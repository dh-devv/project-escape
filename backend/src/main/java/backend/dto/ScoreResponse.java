package backend.dto;

public class ScoreResponse {

    private final int recordId;
    private final int userId;
    private final int bossId;
    private final double clearTime;
    private final int score;
    private final int maxPhase;

    public ScoreResponse(
            int recordId,
            int userId,
            int bossId,
            double clearTime,
            int score,
            int maxPhase
    ) {
        this.recordId = recordId;
        this.userId = userId;
        this.bossId = bossId;
        this.clearTime = clearTime;
        this.score = score;
        this.maxPhase = maxPhase;
    }

    public int getRecordId() {
        return recordId;
    }

    public int getUserId() {
        return userId;
    }

    public int getBossId() {
        return bossId;
    }

    public double getClearTime() {
        return clearTime;
    }

    public int getScore() {
        return score;
    }

    public int getMaxPhase() {
        return maxPhase;
    }
}