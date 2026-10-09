package backend.persistence;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.domain.Pageable;

import java.util.List;
import java.util.Optional;

public interface StoredGameResultRepository
        extends JpaRepository<StoredGameResult, Integer> {

    List<StoredGameResult> findAllBy(Pageable pageable);

    Optional<StoredGameResult> findFirstByUser_UserIdOrderByScoreDescClearTimeAscRecordIdAsc(
            Integer userId
    );
}
