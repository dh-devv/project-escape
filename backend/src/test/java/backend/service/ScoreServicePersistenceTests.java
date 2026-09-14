package backend.service;

import backend.dto.ScoreRequest;
import backend.dto.ScoreResponse;
import backend.dto.UserRequest;
import backend.persistence.StoredGameResultRepository;
import backend.persistence.StoredUserRepository;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;

@SpringBootTest
@ActiveProfiles("test")
class ScoreServicePersistenceTests {

    @Autowired
    private UserService userService;

    @Autowired
    private ScoreService scoreService;

    @Autowired
    private StoredUserRepository userRepository;

    @Autowired
    private StoredGameResultRepository resultRepository;

    @BeforeEach
    void clearDatabase() {
        resultRepository.deleteAll();
        userRepository.deleteAll();
    }

    @Test
    void savesScoreAndReturnsRankingFromDatabase() {
        UserRequest userRequest = new UserRequest();
        userRequest.setUsername("player1");
        int userId = userService.createUser(userRequest).getUserId();

        scoreService.saveScore(scoreRequest(userId, 100));
        scoreService.saveScore(scoreRequest(userId, 200));

        List<ScoreResponse> ranking = scoreService.getRanking(10);

        assertThat(ranking).hasSize(2);
        assertThat(ranking.get(0).getScore()).isEqualTo(200);
        assertThat(scoreService.getBestScore(userId).getScore())
                .isEqualTo(200);
    }

    private ScoreRequest scoreRequest(int userId, int score) {
        ScoreRequest request = new ScoreRequest();
        request.setUserId(userId);
        request.setBossId(1);
        request.setClearTime(120.0);
        request.setScore(score);
        request.setMaxPhase(3);
        return request;
    }
}