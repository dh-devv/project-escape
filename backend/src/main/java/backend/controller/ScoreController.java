package backend.controller;

import backend.dto.ScoreRequest;
import backend.dto.ScoreResponse;
import backend.service.ScoreService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/v1/scores")
public class ScoreController {

    private final ScoreService scoreService;

    public ScoreController(ScoreService scoreService) {
        this.scoreService = scoreService;
    }

    @PostMapping
    public ResponseEntity<ScoreResponse> saveScore(
            @RequestBody ScoreRequest request
    ) {

        ScoreResponse response =
                scoreService.saveScore(request);

        return ResponseEntity.ok(response);
    }
}