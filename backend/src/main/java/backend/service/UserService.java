package backend.service;

import backend.dto.UserRequest;
import backend.dto.UserResponse;
import backend.exception.UserNotFoundException;
import backend.persistence.StoredUser;
import backend.persistence.StoredUserRepository;
import org.springframework.stereotype.Service;
import org.springframework.dao.DataIntegrityViolationException;

@Service
public class UserService {

    private final StoredUserRepository userRepository;

    public UserService(StoredUserRepository userRepository) {
        this.userRepository = userRepository;
    }

    public UserResponse createUser(UserRequest request) {
        String username = request.getUsername().strip();
        if (userRepository.existsByUsername(username)) {
            throw new IllegalArgumentException("username is already in use");
        }

        StoredUser user;
        try {
            user = userRepository.saveAndFlush(new StoredUser(username));
        } catch (DataIntegrityViolationException exception) {
            // The database unique key also covers simultaneous create requests.
            if (userRepository.existsByUsername(username)) {
                throw new IllegalArgumentException("username is already in use", exception);
            }
            throw exception;
        }

        return new UserResponse(
                user.getUserId(),
                user.getUsername()
        );
    }

    public boolean exists(int userId) {
        return userRepository.existsById(userId);
    }

    public StoredUser findById(int userId) {
        return userRepository.findById(userId)
                .orElseThrow(() -> new UserNotFoundException(userId));
    }
}
