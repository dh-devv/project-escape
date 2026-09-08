package backend.service;

import backend.dto.ScoreRequest;
import backend.dto.ScoreResponse;
import backend.model.GameResult;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;

@Service
public class ScoreService {

    private final Map<Integer, GameResult> results =
            new ConcurrentHashMap<>();

    private final AtomicInteger recordIdSequence =
            new AtomicInteger(1);

    private final UserService userService;

    public ScoreService(UserService userService) {
        this.userService = userService;
    }

    public ScoreResponse saveScore(ScoreRequest request) {

        if (!userService.exists(request.getUserId())) {
            throw new IllegalArgumentException(
                    "User not found: " + request.getUserId()
            );
        }

        int recordId =
                recordIdSequence.getAndIncrement();

        GameResult result = new GameResult(
                recordId,
                request.getUserId(),
                request.getBossId(),
                request.getClearTime(),
                request.getScore(),
                request.getMaxPhase()
        );

        results.put(recordId, result);

        return toResponse(result);
    }

    public List<ScoreResponse> getRanking(int limit) {

        List<GameResult> sortedResults =
                new ArrayList<>(results.values());

        sortedResults.sort(
                Comparator
                        .comparingInt(GameResult::getScore)
                        .reversed()
                        .thenComparingDouble(
                                GameResult::getClearTime
                        )
        );

        return sortedResults.stream()
                .limit(limit)
                .map(this::toResponse)
                .toList();
    }

    public ScoreResponse getBestScore(int userId) {

        return results.values()
                .stream()
                .filter(result ->
                        result.getUserId() == userId
                )
                .sorted(
                        Comparator
                                .comparingInt(
                                        GameResult::getScore
                                )
                                .reversed()
                                .thenComparingDouble(
                                        GameResult::getClearTime
                                )
                )
                .findFirst()
                .map(this::toResponse)
                .orElse(null);
    }

    private ScoreResponse toResponse(GameResult result) {

        return new ScoreResponse(
                result.getRecordId(),
                result.getUserId(),
                result.getBossId(),
                result.getClearTime(),
                result.getScore(),
                result.getMaxPhase()
        );
    }
}