package backend;

import com.jayway.jsonpath.JsonPath;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.condition.EnabledIfEnvironmentVariable;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.test.context.ActiveProfiles;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;

/** Opt-in check: validates the existing schema and removes only its own temporary records. */
@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT,
        properties = "spring.jpa.hibernate.ddl-auto=validate")
@ActiveProfiles("mysqlcheck")
@EnabledIfEnvironmentVariable(named = "VERIFY_MYSQL", matches = "true")
class MySqlIntegrationTests {
    @LocalServerPort private int port;
    @Autowired private JdbcTemplate jdbc;
    private final HttpClient client = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(5)).build();

    @Test
    void savesAndReadsUnityResultInMySqlAndCleansUpItsOwnRows() throws Exception {
        assertThat(jdbc.queryForObject("SELECT VERSION()", String.class)).doesNotContain("H2");
        String username = "check_" + UUID.randomUUID();
        try {
            var user = request("/users", "{\"username\":\"" + username + "\"}");
            assertThat(user.statusCode()).isEqualTo(200);
            int userId = JsonPath.read(user.body(), "$.userId");
            var score = request("/scores", "{\"userId\":" + userId +
                    ",\"bossId\":2,\"clearTime\":70.25,\"score\":1000,\"maxPhase\":3}");
            assertThat(score.statusCode()).isEqualTo(200);
            int recordId = JsonPath.read(score.body(), "$.recordId");
            assertThat(jdbc.queryForObject("SELECT max_phase FROM game_results WHERE record_id = ?",
                    Integer.class, recordId)).isEqualTo(3);
            var best = request("/users/" + userId + "/best", null);
            assertThat(best.statusCode()).isEqualTo(200);
            assertThat((Integer) JsonPath.read(best.body(), "$.recordId")).isEqualTo(recordId);
            assertThat((Double) JsonPath.read(best.body(), "$.clearTime")).isEqualTo(70.25);
            assertThat(request("/ranks?limit=1", null).statusCode()).isEqualTo(200);
            assertThat(request("/users", "{\"username\":\"" + username + "\"}").statusCode()).isEqualTo(400);
        } finally {
            jdbc.update("DELETE r FROM game_results r JOIN users u ON r.user_id = u.user_id WHERE u.username = ?", username);
            jdbc.update("DELETE FROM users WHERE username = ?", username);
            assertThat(jdbc.queryForObject("SELECT COUNT(*) FROM users WHERE username = ?",
                    Integer.class, username)).isZero();
        }
    }

    private HttpResponse<String> request(String route, String json) throws Exception {
        var builder = HttpRequest.newBuilder(URI.create("http://localhost:" + port + "/api/v1" + route))
                .timeout(Duration.ofSeconds(10));
        if (json == null) builder.GET();
        else builder.header("Content-Type", "application/json").POST(HttpRequest.BodyPublishers.ofString(json));
        return client.send(builder.build(), HttpResponse.BodyHandlers.ofString());
    }
}
