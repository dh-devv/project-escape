package backend.controller;

import backend.dto.UserRequest;
import backend.dto.UserResponse;
import backend.service.UserService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/v1/users")
public class UserController {

    private final UserService userService;

    public UserController(UserService userService) {
        this.userService = userService;
    }

    @PostMapping
    public ResponseEntity<UserResponse> createUser(
            @RequestBody UserRequest request
    ) {

        UserResponse response =
                userService.createUser(request);

        return ResponseEntity.ok(response);
    }
}