package backend.persistence;

import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface StoredUserRepository extends JpaRepository<StoredUser, Integer> {

    boolean existsByUsername(String username);

    Optional<StoredUser> findById(Integer userId);
}