package backend.service;

import backend.dto.ScoreRequest;
import backend.dto.ScoreResponse;
import backend.persistence.StoredGameResult;
import backend.persistence.StoredGameResultRepository;
import backend.persistence.StoredUser;
import org.springframework.stereotype.Service;
import org.springframework.data.domain.Sort;

import java.util.List;

@Service
public class ScoreService {

    private final UserService userService;
    private final StoredGameResultRepository resultRepository;

    public ScoreService(
            UserService userService,
            StoredGameResultRepository resultRepository
    ) {
        this.userService = userService;
        this.resultRepository = resultRepository;
    }

    public ScoreResponse saveScore(ScoreRequest request) {

        StoredUser user = userService.findById(request.getUserId());

        StoredGameResult result = new StoredGameResult(
                user,
                request.getBossId(),
                request.getClearTime(),
                request.getScore(),
                request.getMaxPhase()
        );

        result = resultRepository.save(result);

        return toResponse(result);
    }

    public List<ScoreResponse> getRanking(int limit) {
        Sort sort = Sort.by(
                Sort.Order.desc("score"),
                Sort.Order.asc("clearTime")
        );

        return resultRepository.findAll(sort).stream()
                .limit(limit)
                .map(this::toResponse)
                .toList();
    }

    public ScoreResponse getBestScore(int userId) {

        return resultRepository
                .findByUser_UserIdOrderByScoreDescClearTimeAsc(userId)
                .stream()
                .findFirst()
                .map(this::toResponse)
                .orElse(null);
    }

    private ScoreResponse toResponse(StoredGameResult result) {

        return new ScoreResponse(
                result.getRecordId(),
                result.getUser().getUserId(),
                result.getBossId(),
                result.getClearTime(),
                result.getScore(),
                result.getMaxPhase()
        );
    }
}