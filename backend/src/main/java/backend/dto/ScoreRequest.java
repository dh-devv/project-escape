package backend.dto;

public class ScoreRequest {

    private int userId;
    private int bossId;
    private double clearTime;
    private int score;
    private int maxPhase;

    public ScoreRequest() {
    }

    public int getUserId() {
        return userId;
    }

    public void setUserId(int userId) {
        this.userId = userId;
    }

    public int getBossId() {
        return bossId;
    }

    public void setBossId(int bossId) {
        this.bossId = bossId;
    }

    public double getClearTime() {
        return clearTime;
    }

    public void setClearTime(double clearTime) {
        this.clearTime = clearTime;
    }

    public int getScore() {
        return score;
    }

    public void setScore(int score) {
        this.score = score;
    }

    public int getMaxPhase() {
        return maxPhase;
    }

    public void setMaxPhase(int maxPhase) {
        this.maxPhase = maxPhase;
    }
}
