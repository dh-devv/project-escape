package backend.controller;

import backend.dto.ScoreResponse;
import backend.service.ScoreService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/v1")
public class RankingController {

    private final ScoreService scoreService;

    public RankingController(ScoreService scoreService) {
        this.scoreService = scoreService;
    }

    @GetMapping("/ranks")
    public ResponseEntity<List<ScoreResponse>> getRanking(
            @RequestParam(defaultValue = "10") int limit
    ) {

        if (limit < 1) {
            limit = 10;
        }

        List<ScoreResponse> ranking =
                scoreService.getRanking(limit);

        return ResponseEntity.ok(ranking);
    }

    @GetMapping("/users/{userId}/best")
    public ResponseEntity<?> getBestScore(
            @PathVariable int userId
    ) {

        ScoreResponse response =
                scoreService.getBestScore(userId);

        if (response == null) {
            return ResponseEntity.notFound().build();
        }

        return ResponseEntity.ok(response);
    }
}`