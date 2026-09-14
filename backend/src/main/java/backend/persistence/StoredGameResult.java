package backend.persistence;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.FetchType;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.Table;

@Entity
@Table(name = "game_results")
public class StoredGameResult {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer recordId;

    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @JoinColumn(name = "user_id", nullable = false)
    private StoredUser user;

    @Column(name = "boss_id", nullable = false)
    private Integer bossId;

    @Column(name = "clear_time", nullable = false)
    private Double clearTime;

    @Column(nullable = false)
    private Integer score;

    @Column(name = "max_phase", nullable = false)
    private Integer maxPhase;

    protected StoredGameResult() {
    }

    public StoredGameResult(
            StoredUser user,
            Integer bossId,
            Double clearTime,
            Integer score,
            Integer maxPhase
    ) {
        this.user = user;
        this.bossId = bossId;
        this.clearTime = clearTime;
        this.score = score;
        this.maxPhase = maxPhase;
    }

    public Integer getRecordId() {
        return recordId;
    }

    public StoredUser getUser() {
        return user;
    }

    public Integer getBossId() {
        return bossId;
    }

    public Double getClearTime() {
        return clearTime;
    }

    public Integer getScore() {
        return score;
    }

    public Integer getMaxPhase() {
        return maxPhase;
    }
}