package backend.service;

import backend.dto.UserRequest;
import backend.dto.UserResponse;
import backend.model.User;
import org.springframework.stereotype.Service;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;

@Service
public class UserService {

    private final Map<Integer, User> users = new ConcurrentHashMap<>();

    private final AtomicInteger userIdSequence =
            new AtomicInteger(1);

    public UserResponse createUser(UserRequest request) {

        if (request.getUsername() == null ||
                request.getUsername().isBlank()) {

            throw new IllegalArgumentException(
                    "username is required"
            );
        }

        int userId = userIdSequence.getAndIncrement();

        User user = new User(
                userId,
                request.getUsername()
        );

        users.put(userId, user);

        return new UserResponse(
                user.getUserId(),
                user.getUsername()
        );
    }

    public boolean exists(int userId) {
        return users.containsKey(userId);
    }

    public User findById(int userId) {
        return users.get(userId);
    }
}