package backend.dto;

import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Positive;
import jakarta.validation.constraints.PositiveOrZero;

public class ScoreRequest {

    @NotNull(message = "userId is required")
    @Positive(message = "userId must be positive")
    private Integer userId;

    @NotNull(message = "bossId is required")
    @Positive(message = "bossId must be positive")
    private Integer bossId;

    @NotNull(message = "clearTime is required")
    @Positive(message = "clearTime must be positive")
    private Double clearTime;

    @NotNull(message = "score is required")
    @PositiveOrZero(message = "score must not be negative")
    private Integer score;

    @NotNull(message = "maxPhase is required")
    @Min(value = 1, message = "maxPhase must be between 1 and 3")
    @Max(value = 3, message = "maxPhase must be between 1 and 3")
    private Integer maxPhase;

    public ScoreRequest() {
    }

    public int getUserId() {
        return userId;
    }

    public void setUserId(Integer userId) {
        this.userId = userId;
    }

    public int getBossId() {
        return bossId;
    }

    public void setBossId(Integer bossId) {
        this.bossId = bossId;
    }

    public double getClearTime() {
        return clearTime;
    }

    public void setClearTime(Double clearTime) {
        this.clearTime = clearTime;
    }

    public int getScore() {
        return score;
    }

    public void setScore(Integer score) {
        this.score = score;
    }

    public int getMaxPhase() {
        return maxPhase;
    }

    public void setMaxPhase(Integer maxPhase) {
        this.maxPhase = maxPhase;
    }
}
