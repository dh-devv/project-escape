import json
import os
import re
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import unquote, urlsplit
from uuid import UUID


PROFILE_COLUMNS = "profile_id, runs, play_seconds, best_room, best_phase, best_clear_seconds"


def validate_profile_id(value):
    try:
        return str(UUID(str(value)))
    except (ValueError, TypeError, AttributeError):
        raise ValueError("profileId must be a UUID")


def _integer(value, name, minimum, maximum):
    if isinstance(value, bool):
        raise ValueError(name + " must be an integer")
    try:
        number = int(value)
    except (ValueError, TypeError, OverflowError):
        raise ValueError(name + " must be an integer")
    try:
        if float(value) != number:
            raise ValueError(name + " must be an integer")
    except (ValueError, TypeError, OverflowError):
        raise ValueError(name + " must be an integer")
    if number < minimum or number > maximum:
        raise ValueError(name + " is out of range")
    return number


def validate_profile(payload):
    if not isinstance(payload, dict):
        raise ValueError("profile must be an object")
    best_clear = payload.get("bestClearSeconds")
    if best_clear is not None:
        best_clear = _integer(best_clear, "bestClearSeconds", 1, 604800)
    return {
        "runs": _integer(payload.get("runs"), "runs", 0, 10000000),
        "playSeconds": _integer(payload.get("playSeconds"), "playSeconds", 0, 2147483647),
        "bestRoom": _integer(payload.get("bestRoom"), "bestRoom", 0, 20),
        "bestPhase": _integer(payload.get("bestPhase"), "bestPhase", 0, 3),
        "bestClearSeconds": best_clear,
    }


def connect_database():
    import mysql.connector

    return mysql.connector.connect(
        host=os.environ.get("DB_HOST", "db"),
        port=int(os.environ.get("DB_PORT", "3306")),
        user=os.environ["DB_USER"],
        password=os.environ["DB_PASSWORD"],
        database=os.environ.get("DB_NAME", "lastspark_game"),
        connection_timeout=5,
    )


def _as_profile(row):
    if not row:
        return None
    return {
        "profileId": row[0],
        "runs": int(row[1]),
        "playSeconds": int(row[2]),
        "bestRoom": int(row[3]),
        "bestPhase": int(row[4]),
        "bestClearSeconds": int(row[5]) if row[5] is not None else None,
    }


def get_profile(profile_id, connection_factory=connect_database):
    profile_id = validate_profile_id(profile_id)
    connection = connection_factory()
    cursor = None
    try:
        cursor = connection.cursor()
        cursor.execute("SELECT " + PROFILE_COLUMNS + " FROM game_profiles WHERE profile_id = %s", (profile_id,))
        return _as_profile(cursor.fetchone())
    finally:
        try:
            if cursor is not None:
                cursor.close()
        finally:
            connection.close()


def sync_profile(profile_id, payload, connection_factory=connect_database):
    profile_id = validate_profile_id(profile_id)
    profile = validate_profile(payload)
    connection = connection_factory()
    cursor = None
    try:
        cursor = connection.cursor()
        cursor.execute(
            "INSERT INTO game_profiles "
            "(profile_id, runs, play_seconds, best_room, best_phase, best_clear_seconds) "
            "VALUES (%s, %s, %s, %s, %s, %s) AS incoming "
            "ON DUPLICATE KEY UPDATE "
            "runs = GREATEST(game_profiles.runs, incoming.runs), "
            "play_seconds = GREATEST(game_profiles.play_seconds, incoming.play_seconds), "
            "best_room = GREATEST(game_profiles.best_room, incoming.best_room), "
            "best_phase = GREATEST(game_profiles.best_phase, incoming.best_phase), "
            "best_clear_seconds = CASE "
            "WHEN game_profiles.best_clear_seconds IS NULL THEN incoming.best_clear_seconds "
            "WHEN incoming.best_clear_seconds IS NULL THEN game_profiles.best_clear_seconds "
            "ELSE LEAST(game_profiles.best_clear_seconds, incoming.best_clear_seconds) END",
            (
                profile_id,
                profile["runs"],
                profile["playSeconds"],
                profile["bestRoom"],
                profile["bestPhase"],
                profile["bestClearSeconds"],
            ),
        )
        connection.commit()
        cursor.execute("SELECT " + PROFILE_COLUMNS + " FROM game_profiles WHERE profile_id = %s", (profile_id,))
        return _as_profile(cursor.fetchone())
    finally:
        try:
            if cursor is not None:
                cursor.close()
        finally:
            connection.close()


class ApiHandler(BaseHTTPRequestHandler):
    server_version = "LastSparkGameApi/1.0"

    def log_message(self, format_string, *args):
        print("%s - %s" % (self.address_string(), format_string % args))

    def send_json(self, status, body):
        encoded = json.dumps(body, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(encoded)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(encoded)

    def do_GET(self):
        path = urlsplit(self.path).path
        if path == "/api/health":
            connection = None
            cursor = None
            try:
                connection = connect_database()
                cursor = connection.cursor()
                cursor.execute("SELECT 1")
                cursor.fetchone()
                self.send_json(200, {"status": "ok", "database": "connected"})
            except Exception:
                self.send_json(503, {"status": "unavailable", "database": "disconnected"})
            finally:
                try:
                    if cursor is not None:
                        cursor.close()
                finally:
                    if connection is not None:
                        connection.close()
            return
        match = re.fullmatch(r"/api/v1/profiles/([^/]+)", path)
        if match:
            try:
                profile = get_profile(unquote(match.group(1)))
                if profile is None:
                    self.send_json(404, {"error": "profile_not_found"})
                else:
                    self.send_json(200, profile)
            except ValueError as error:
                self.send_json(400, {"error": "invalid_profile", "message": str(error)})
            except Exception:
                self.send_json(503, {"error": "storage_unavailable"})
            return
        self.send_json(404, {"error": "not_found"})

    def do_POST(self):
        if urlsplit(self.path).path != "/api/v1/profiles/sync":
            self.send_json(404, {"error": "not_found"})
            return
        try:
            length = int(self.headers.get("Content-Length", "0"))
            if length <= 0 or length > 8192:
                raise ValueError("request body size is invalid")
            payload = json.loads(self.rfile.read(length).decode("utf-8"))
            if not isinstance(payload, dict):
                raise ValueError("request body must be an object")
            profile = sync_profile(payload.get("profileId"), payload.get("profile"))
            self.send_json(200, profile)
        except (ValueError, UnicodeDecodeError, json.JSONDecodeError) as error:
            self.send_json(400, {"error": "invalid_request", "message": str(error)})
        except Exception:
            self.send_json(503, {"error": "storage_unavailable"})

    def do_OPTIONS(self):
        self.send_response(204)
        self.send_header("Allow", "GET, POST, OPTIONS")
        self.end_headers()


def main():
    address = ("0.0.0.0", int(os.environ.get("PORT", "3000")))
    print("Game profile API listening on %s:%s" % address, flush=True)
    ThreadingHTTPServer(address, ApiHandler).serve_forever()


if __name__ == "__main__":
    main()
