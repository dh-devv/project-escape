package backend;

import backend.persistence.StoredGameResultRepository;
import backend.persistence.StoredUserRepository;
import com.jayway.jsonpath.JsonPath;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.test.context.ActiveProfiles;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;

import static org.assertj.core.api.Assertions.assertThat;

/** Real HTTP requests using the same JSON fields sent by LastSparkApiClient. */
@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
@ActiveProfiles("test")
class ApiDatabaseIntegrationTests {
    @LocalServerPort private int port;
    @Autowired private StoredUserRepository users;
    @Autowired private StoredGameResultRepository results;
    private final HttpClient client = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(5)).build();

    @BeforeEach
    void clearDatabase() {
        results.deleteAll();
        users.deleteAll();
    }

    @Test
    void unityContractPersistsBossResultAndReturnsRankingAndBestScore() throws Exception {
        var user = post("/users", "{\"username\":\"unity-player\"}");
        assertThat(user.statusCode()).isEqualTo(200);
        int userId = JsonPath.read(user.body(), "$.userId");
        assertThat(users.findById(userId)).isPresent();

        var first = post("/scores", score(userId, 1, 125.5, 1000, 2));
        var second = post("/scores", score(userId, 2, 70.25, 1000, 3));
        assertThat(first.statusCode()).isEqualTo(200);
        assertThat(second.statusCode()).isEqualTo(200);
        assertThat(results.count()).isEqualTo(2);
        int recordId = JsonPath.read(second.body(), "$.recordId");

        var ranks = get("/ranks?limit=1");
        assertThat(ranks.statusCode()).isEqualTo(200);
        assertThat((Integer) JsonPath.read(ranks.body(), "$.length()")).isEqualTo(1);
        assertThat((Integer) JsonPath.read(ranks.body(), "$[0].recordId")).isEqualTo(recordId);
        assertThat((Integer) JsonPath.read(ranks.body(), "$[0].maxPhase")).isEqualTo(3);
        var best = get("/users/" + userId + "/best");
        assertThat(best.statusCode()).isEqualTo(200);
        assertThat((Integer) JsonPath.read(best.body(), "$.recordId")).isEqualTo(recordId);
        assertThat((Double) JsonPath.read(best.body(), "$.clearTime")).isEqualTo(70.25);
    }

    @Test
    void invalidRequestsDoNotCreateOrphanedOrInvalidResults() throws Exception {
        assertThat(post("/users", "{\"username\":\"   \"}").statusCode()).isEqualTo(400);
        assertThat(post("/users", "{invalid}").statusCode()).isEqualTo(400);
        assertThat(post("/scores", score(999999, 1, 10, 100, 2)).statusCode()).isEqualTo(404);
        assertThat(post("/scores", score(999999, 1, 0, -1, 4)).statusCode()).isEqualTo(400);
        assertThat(results.count()).isZero();
        assertThat(users.count()).isZero();

        var user = post("/users", "{\"username\":\"unique-player\"}");
        int userId = JsonPath.read(user.body(), "$.userId");
        assertThat(post("/users", "{\"username\":\"unique-player\"}").statusCode()).isEqualTo(400);
        assertThat(get("/users/" + userId + "/best").statusCode()).isEqualTo(404);
        assertThat(users.count()).isEqualTo(1);
    }

    @Test
    void normalizesUsernamesAndRejectsDuplicateNormalizedName() throws Exception {
        var response = post("/users", "{\"username\":\"  player  \"}");
        assertThat(response.statusCode()).isEqualTo(200);
        assertThat((String) JsonPath.read(response.body(), "$.username")).isEqualTo("player");
        assertThat(post("/users", "{\"username\":\"player\"}").statusCode()).isEqualTo(400);
        assertThat(users.count()).isEqualTo(1);
    }

    @Test
    void rejectsNonFiniteTimesAndReturnsConsistentParameterErrors() throws Exception {
        var response = post("/users", "{\"username\":\"finite-player\"}");
        int userId = JsonPath.read(response.body(), "$.userId");
        String nonFinite = score(userId, 1, 1, 100, 1).replace("\"clearTime\":1.0", "\"clearTime\":\"Infinity\"");
        assertThat(post("/scores", nonFinite).statusCode()).isEqualTo(400);
        assertThat(results.count()).isZero();
        var invalidLimit = get("/ranks?limit=invalid");
        assertThat(invalidLimit.statusCode()).isEqualTo(400);
        assertThat((String) JsonPath.read(invalidLimit.body(), "$.code")).isEqualTo("INVALID_REQUEST");
    }

    @Test
    void ranksEqualScoresDeterministicallyAndLimitsResults() throws Exception {
        var response = post("/users", "{\"username\":\"ranking-player\"}");
        int userId = JsonPath.read(response.body(), "$.userId");
        var first = post("/scores", score(userId, 1, 10, 100, 1));
        post("/scores", score(userId, 1, 10, 100, 1));
        post("/scores", score(userId, 1, 20, 100, 1));
        post("/scores", score(userId, 1, 1, 99, 1));
        int firstId = JsonPath.read(first.body(), "$.recordId");
        assertThat((Integer) JsonPath.read(get("/ranks?limit=2").body(), "$.length()")).isEqualTo(2);
        assertThat((Integer) JsonPath.read(get("/ranks?limit=2").body(), "$[0].recordId")).isEqualTo(firstId);
        assertThat((Integer) JsonPath.read(get("/users/" + userId + "/best").body(), "$.recordId")).isEqualTo(firstId);
        assertThat((Integer) JsonPath.read(get("/ranks?limit=0").body(), "$.length()")).isEqualTo(4);
    }

    private String score(int userId, int bossId, double seconds, int score, int phase) {
        return "{\"userId\":" + userId + ",\"bossId\":" + bossId +
                ",\"clearTime\":" + seconds + ",\"score\":" + score + ",\"maxPhase\":" + phase + "}";
    }
    private HttpResponse<String> post(String route, String json) throws Exception {
        return client.send(HttpRequest.newBuilder(uri(route)).timeout(Duration.ofSeconds(5)).header("Content-Type", "application/json")
                .POST(HttpRequest.BodyPublishers.ofString(json)).build(), HttpResponse.BodyHandlers.ofString());
    }
    private HttpResponse<String> get(String route) throws Exception {
        return client.send(HttpRequest.newBuilder(uri(route)).timeout(Duration.ofSeconds(5)).GET().build(), HttpResponse.BodyHandlers.ofString());
    }
    private URI uri(String route) { return URI.create("http://localhost:" + port + "/api/v1" + route); }
}
