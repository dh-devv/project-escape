package backend.service;

import backend.dto.UserRequest;
import backend.dto.UserResponse;
import backend.exception.UserNotFoundException;
import backend.persistence.StoredUser;
import backend.persistence.StoredUserRepository;
import org.springframework.stereotype.Service;

@Service
public class UserService {

    private final StoredUserRepository userRepository;

    public UserService(StoredUserRepository userRepository) {
    this.userRepository = userRepository;
    }

    public UserResponse createUser(UserRequest request) {
    if (userRepository.existsByUsername(request.getUsername())) {
        throw new IllegalArgumentException("username is already in use");
        }

    StoredUser user = userRepository.save(
        new StoredUser(request.getUsername())
    );

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