import pathlib
import sys
import json
import threading
import unittest
from unittest.mock import patch
from urllib.error import HTTPError
from urllib.request import Request, urlopen
from http.server import ThreadingHTTPServer

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parents[1]))
import app


class FakeCursor:
    def __init__(self):
        self.statements = []
        self.row = None

    def execute(self, statement, params=None):
        self.statements.append((statement, params))
        if statement.startswith("SELECT"):
            self.row = ("123e4567-e89b-12d3-a456-426614174000", 6, 912, 5, 3, 48)

    def fetchone(self):
        return self.row

    def close(self):
        pass


class FakeConnection:
    def __init__(self):
        self.cursor_instance = FakeCursor()
        self.committed = False
        self.closed = False

    def cursor(self):
        return self.cursor_instance

    def commit(self):
        self.committed = True

    def close(self):
        self.closed = True


class ProfileApiTests(unittest.TestCase):
    def setUp(self):
        self.payload = {
            "runs": 6,
            "playSeconds": 912,
            "bestRoom": 5,
            "bestPhase": 3,
            "bestClearSeconds": 48,
        }

    def test_profile_payload_accepts_saved_game_fields(self):
        self.assertEqual(app.validate_profile(self.payload), self.payload)

    def test_profile_payload_rejects_invalid_phase(self):
        invalid = dict(self.payload, bestPhase=4)
        with self.assertRaises(ValueError):
            app.validate_profile(invalid)

    def test_profile_payload_rejects_negative_play_time(self):
        invalid = dict(self.payload, playSeconds=-1)
        with self.assertRaises(ValueError):
            app.validate_profile(invalid)

    def test_profile_payload_allows_missing_clear_time(self):
        self.assertIsNone(app.validate_profile(dict(self.payload, bestClearSeconds=None))["bestClearSeconds"])

    def test_profile_id_must_be_a_uuid(self):
        with self.assertRaises(ValueError):
            app.validate_profile_id("../../admin")

    def test_sync_upserts_mysql_record_and_returns_saved_values(self):
        connection = FakeConnection()
        saved = app.sync_profile(
            "123e4567-e89b-12d3-a456-426614174000",
            self.payload,
            connection_factory=lambda: connection,
        )
        self.assertTrue(connection.committed)
        self.assertTrue(connection.closed)
        statements = " ".join(sql for sql, _ in connection.cursor_instance.statements)
        self.assertIn("ON DUPLICATE KEY UPDATE", statements)
        self.assertIn("GREATEST(game_profiles.runs, incoming.runs)", statements)
        self.assertEqual(saved["playSeconds"], 912)
        self.assertEqual(saved["bestClearSeconds"], 48)


class HttpRouteTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.server = ThreadingHTTPServer(("127.0.0.1", 0), app.ApiHandler)
        cls.thread = threading.Thread(target=cls.server.serve_forever, daemon=True)
        cls.thread.start()
        cls.base = "http://127.0.0.1:%s" % cls.server.server_address[1]

    @classmethod
    def tearDownClass(cls):
        cls.server.shutdown()
        cls.server.server_close()
        cls.thread.join(timeout=2)

    def test_unknown_route_returns_not_found(self):
        with self.assertRaises(HTTPError) as error:
            urlopen(self.base + "/api/not-a-route", timeout=2)
        self.assertEqual(error.exception.code, 404)

    def test_health_route_checks_database_connection(self):
        connection = FakeConnection()
        with patch.object(app, "connect_database", return_value=connection):
            response = urlopen(self.base + "/api/health", timeout=2)
            self.assertEqual(response.status, 200)
            self.assertEqual(json.loads(response.read()), {"status": "ok", "database": "connected"})

    def test_sync_route_rejects_invalid_profile_without_database_access(self):
        request = Request(
            self.base + "/api/v1/profiles/sync",
            data=json.dumps({"profileId": "invalid", "profile": {}}).encode("utf-8"),
            headers={"Content-Type": "application/json"},
            method="POST",
        )
        with self.assertRaises(HTTPError) as error:
            urlopen(request, timeout=2)
        self.assertEqual(error.exception.code, 400)


if __name__ == "__main__":
    unittest.main()
