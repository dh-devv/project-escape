package backend.dto;

public class UserResponse {

    private final int userId;
    private final String username;

    public UserResponse(int userId, String username) {
        this.userId = userId;
        this.username = username;
    }

    public int getUserId() {
        return userId;
    }

    public String getUsername() {
        return username;
    }
}