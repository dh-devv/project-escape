-- LAST SPARK 데이터베이스 생성 스크립트
-- MySQL 8.0 이상에서 실행하는 것을 권장합니다.
-- 이 파일은 데이터베이스와 게임에 필요한 두 개의 테이블을 만듭니다.

CREATE DATABASE IF NOT EXISTS last_spark
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE last_spark;

-- 플레이어 정보를 저장하는 테이블입니다.
CREATE TABLE IF NOT EXISTS users (
    user_id INT NOT NULL AUTO_INCREMENT COMMENT '플레이어 고유 번호',
    username VARCHAR(50) NOT NULL COMMENT '플레이어 이름',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '플레이어 생성 시간',
    PRIMARY KEY (user_id),
    UNIQUE KEY uk_users_username (username)
) ENGINE=InnoDB;

-- 보스를 클리어한 결과를 저장하는 테이블입니다.
-- user_id를 이용해 users 테이블의 플레이어와 연결합니다.
CREATE TABLE IF NOT EXISTS game_results (
    record_id INT NOT NULL AUTO_INCREMENT COMMENT '게임 결과 고유 번호',
    user_id INT NOT NULL COMMENT '플레이어 고유 번호',
    boss_id INT NOT NULL COMMENT '보스 고유 번호',
    clear_time DOUBLE NOT NULL COMMENT '클리어 시간. 단위는 초',
    score INT NOT NULL COMMENT '게임 점수',
    max_phase INT NOT NULL COMMENT '게임 중 도달한 가장 높은 페이즈',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '결과 저장 시간',
    PRIMARY KEY (record_id),
    
    -- 점수가 높은 순서로 랭킹을 빠르게 조회하기 위한 인덱스입니다.
    KEY idx_game_results_ranking (score DESC, clear_time ASC),

    -- 특정 플레이어의 최고 기록을 빠르게 조회하기 위한 인덱스입니다.
    KEY idx_game_results_user (user_id),

    -- 존재하지 않는 플레이어의 게임 결과가 저장되지 않도록 연결합니다.
    CONSTRAINT fk_game_results_user
        FOREIGN KEY (user_id) REFERENCES users (user_id)
) ENGINE=InnoDB;