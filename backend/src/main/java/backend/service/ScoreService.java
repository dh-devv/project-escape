package backend.service;

import backend.dto.ScoreRequest;
import backend.dto.ScoreResponse;
import backend.persistence.StoredGameResult;
import backend.persistence.StoredGameResultRepository;
import backend.persistence.StoredUser;
import org.springframework.stereotype.Service;
import org.springframework.data.domain.Sort;
import org.springframework.data.domain.PageRequest;
import org.springframework.transaction.annotation.Transactional;

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

    @Transactional
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

    @Transactional(readOnly = true)
    public List<ScoreResponse> getRanking(int limit) {
        Sort sort = Sort.by(
                Sort.Order.desc("score"),
                Sort.Order.asc("clearTime"),
                Sort.Order.asc("recordId")
        );

        return resultRepository.findAllBy(PageRequest.of(0, Math.max(1, Math.min(limit, 100)), sort)).stream()
                .map(this::toResponse)
                .toList();
    }

    @Transactional(readOnly = true)
    public ScoreResponse getBestScore(int userId) {

        return resultRepository
                .findFirstByUser_UserIdOrderByScoreDescClearTimeAscRecordIdAsc(userId)
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
