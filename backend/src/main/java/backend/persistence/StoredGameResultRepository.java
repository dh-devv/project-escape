package backend.persistence;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface StoredGameResultRepository
        extends JpaRepository<StoredGameResult, Integer> {

    List<StoredGameResult> findByUser_UserIdOrderByScoreDescClearTimeAsc(
            Integer userId
    );
}