// 버프 타입 ID 정의 (같은 ID = 같은 타입의 버프)
// 100단위로 카테고리 구분
public static class BuffType
{
    // ========== 100번대: 공격력 버프 ==========
    public const int DAMAGE_A = 100;        // A타입 공격력 버프 (아군 종류별)
    public const int DAMAGE_B = 101;        // B타입 공격력 버프
    public const int DAMAGE_C = 102;        // C타입 공격력 버프
    public const int DAMAGE_D = 103;        // D타입 공격력 버프
    public const int DAMAGE_E = 104;        // E타입 공격력 버프
    
    // ========== 200번대: 방어력 버프 ==========
    public const int DEFENSE_A = 200;       // A타입 방어력 버프
    public const int DEFENSE_B = 201;       // B타입 방어력 버프
    public const int DEFENSE_C = 202;       // C타입 방어력 버프
    public const int DEFENSE_D = 203;       // D타입 방어력 버프
    
    // ========== 300번대: 공격속도 버프 ==========
    public const int ATTACK_SPEED_A = 300;  // A타입 공격속도 버프
    public const int ATTACK_SPEED_B = 301;  // B타입 공격속도 버프
    public const int ATTACK_SPEED_C = 302;  // C타입 공격속도 버프
    
    // ========== 400번대: 이동속도 버프 ==========
    public const int MOVE_SPEED_A = 400;    // A타입 이동속도 버프
    public const int MOVE_SPEED_B = 401;    // B타입 이동속도 버프
    public const int MOVE_SPEED_C = 402;    // C타입 이동속도 버프
    
    // ========== 500번대: 최대체력 버프 ==========
    public const int MAX_HEALTH_A = 500;    // A타입 최대체력 버프
    public const int MAX_HEALTH_B = 501;    // B타입 최대체력 버프
    public const int MAX_HEALTH_C = 502;    // C타입 최대체력 버프
    
    // ========== 600번대: 크리티컬 확률 ==========
    public const int CRIT_CHANCE_A = 600;   // A타입 크리티컬 확률
    public const int CRIT_CHANCE_B = 601;   // B타입 크리티컬 확률
    public const int CRIT_CHANCE_C = 602;   // C타입 크리티컬 확률
    
    // ========== 700번대: 크리티컬 배수 ==========
    public const int CRIT_MULTIPLIER_A = 700; // A타입 크리티컬 배수
    public const int CRIT_MULTIPLIER_B = 701; // B타입 크리티컬 배수
    
    // ========== 800번대: 데미지 감소 ==========
    public const int DAMAGE_REDUCTION_A = 800; // A타입 데미지 감소
    public const int DAMAGE_REDUCTION_B = 801; // B타입 데미지 감소
    
    // ========== 900번대: 방어 관통 ==========
    public const int PENETRATE_A = 900;     // A타입 방어 관통
    public const int PENETRATE_B = 901;     // B타입 방어 관통
    
    // ========== 1000번대: 최종 데미지 증폭 ==========
    public const int FINAL_DAMAGE_A = 1000; // A타입 최종 데미지 증폭
    public const int FINAL_DAMAGE_B = 1001; // B타입 최종 데미지 증폭
    
    // ========== 1100번대: 이동속도 디버프 (음수 value 사용) ==========
    public const int SLOW_FROST = 1100;     // 동상 이속 감소
    public const int SLOW_FIRE = 1101;      // 화상 이속 감소
    public const int SLOW_POISON = 1102;    // 중독 이속 감소
    public const int SLOW_MUD = 1103;       // 진흙 이속 감소
    public const int SLOW_CURSE = 1104;     // 저주 이속 감소
    
    // ========== 1200번대: 공격력 디버프 (음수 value 사용) ==========
    public const int WEAKEN_A = 1200;       // A타입 공격력 감소
    public const int WEAKEN_B = 1201;       // B타입 공격력 감소
    public const int WEAKEN_C = 1202;       // C타입 공격력 감소
    
    // ========== 1300번대: 방어력 디버프 (음수 value 사용) ==========
    public const int ARMOR_BREAK_A = 1300;  // A타입 방어력 감소
    public const int ARMOR_BREAK_B = 1301;  // B타입 방어력 감소
    public const int ARMOR_BREAK_C = 1302;  // C타입 방어력 감소
    
    // ========== 1400번대: 공격속도 디버프 (음수 value 사용) ==========
    public const int ATTACK_SLOW_A = 1400;  // A타입 공격속도 감소
    public const int ATTACK_SLOW_B = 1401;  // B타입 공격속도 감소
    
    // ========== 1500번대: 회피율 버프 ==========
    public const int DODGE_A = 1500;        // A타입 회피율 증가
    public const int DODGE_B = 1501;        // B타입 회피율 증가
    
    // ========== 1600번대: 명중률 버프 ==========
    public const int ACCURACY_A = 1600;     // A타입 명중률 증가
    public const int ACCURACY_B = 1601;     // B타입 명중률 증가
}