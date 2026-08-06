export function gradeEnumToGradeLetter(grade) {
    switch (grade) {
        case 0:
            return "F";
        case 1:
            return "D";
        case 2:
            return "C";
        case 3:
            return "B";
        case 4:
            return "A";
        case 5:
            return "S";
        case 6:
            return "S";
        case 7:
            return "SS";
        case 8:
            return "SS";
    }
}

export function gradeEnumToGradeClass(grade) {
    switch (grade) {
        case 0:
            return "grade-f";
        case 1:
            return "grade-d";
        case 2:
            return "grade-c";
        case 3:
            return "grade-b";
        case 4:
            return "grade-a";
        case 5:
            return "grade-s";
        case 6:
            return "grade-sh";
        case 7:
            return "grade-x";
        case 8:
            return "grade-xh";
    }
}

function Mod(acronym, name, isRateChange, modes) {
    return {
        acronym: acronym,
        name: name,
        isRateChange: isRateChange,
        modes: modes
    }
}

export const allMods = {
    difficultyDecrease: [
        Mod("EZ", "Easy", false, [0, 1, 2, 3]),
        Mod("NF", "No Fail", false, [0, 1, 2 ,3]),
        Mod("HT", "Half Time", true, [0, 1, 2, 3]),
        Mod("DC", "Daycore", true, [0, 1, 2, 3]),
        Mod("SR", "Simplified Rhythm", false, [1]),
        Mod("NR", "No Release", false, [3])
    ],
    difficultyIncrease: [
        Mod("HR", "Hard Rock", false, [0, 1, 2, 3]),
        Mod("SD", "Sudden Death", false, [0, 1, 2, 3]),
        Mod("PF", "Perfect", false, [0, 1, 2, 3]),
        Mod("DT", "Double Time", true, [0, 1, 2, 3]),
        Mod("NC", "Nightcore", true, [0, 1, 2, 3]),
        Mod("FI", "Fade In", false, [3]),
        Mod("HD", "Hidden", false, [0, 1, 2, 3]),
        Mod("TC", "Traceable", false, [0]),
        Mod("CO", "Cover", false, [3]),
        Mod("FL", "Flashlight", false, [0, 1, 2, 3]),
        Mod("BL", "Blinds", false, [0]),
        Mod("ST", "Strict Tracking", false, [0]),
        Mod("AC", "Accuracy Challenge", false, [0, 1, 2, 3])
    ],
    automation: [
        Mod("RX", "Relax", false, [0, 1, 2]),
        Mod("AP", "Autopilot", false, [0]),
        Mod("SO", "Spun Out", false, [0])
    ],
    conversion: [
        Mod("TP", "Target Practice", false, [0]),
        Mod("DA", "Difficulty Adjust", false, [0, 1, 2, 3]),
        Mod("CL", "Classic", false, [0, 1, 2, 3]),
        Mod("RD", "Random", false, [0, 1, 3]),
        Mod("DS", "Dual Stages", false, [3]),
        Mod("MR", "Mirror", false, [0, 2, 3]),
        Mod("AL", "Alternate", false, [0]),
        Mod("SW", "Swap", false, [1]),
        Mod("SG", "Single Tap", false, [0, 1]),
        Mod("IN", "Invert", false, [3]),
        Mod("CS", "Constant Speed", false, [1, 3]),
        Mod("HO", "Hold Off", false, [3]),
        Mod("1K", "1 Key", false, [3]),
        Mod("2K", "2 Keys", false, [3]),
        Mod("3K", "3 Keys", false, [3]),
        Mod("4K", "4 Keys", false, [3]),
        Mod("5K", "5 Keys", false, [3]),
        Mod("6K", "6 Keys", false, [3]),
        Mod("7K", "7 Keys", false, [3]),
        Mod("8K", "8 Keys", false, [3]),
        Mod("9K", "9 Keys", false, [3]),
        Mod("10K", "10 Keys", false, [3]),
    ],
    fun: [
      Mod("TR", "Transform", false, [0]),
      Mod("WG", "Wiggle", false, [0]),
      Mod("SI", "Spin In", false, [0]),
      Mod("GR", "Grow", false, [0]),
      Mod("DF", "Deflate", false, [0]),
      Mod("WU", "Wind Up", false, [0, 1, 2, 3]),
      Mod("WD", "Wind Down", false, [0, 1, 2, 3]),
      Mod("BR", "Barrel Roll", false, [0]),
      Mod("AD", "Approach Different", false, [0]),
      Mod("FF", "Floating fruits", false, [2]),
      Mod("MU", "Muted", false, [0, 1, 2, 3]),
      Mod("NS", "No Scope", false, [0, 2]),
      Mod("MG", "Magnetised", false, [0, 2]),
      Mod("RP", "Repel", false, [0]),
      Mod("AS", "Adaptive Speed", false, [0, 1, 3]),
      Mod("FR", "Freeze Frame", false, [0]),
      Mod("BU", "Bubbles", false, [0]),
      Mod("MF", "Moving Fast", false, [2]),
      Mod("SY", "Synesthesia", false, [0, 2]),
      Mod("DP", "Depth", false, [0]),
      Mod("BM", "Bloom", false, [0])  
    ],
    system: [
        Mod("TD", "Touch Device", false, [0])
    ]
}

export function getModData(acronym) {
    for (const cat of Object.keys(allMods)) {
        const mod = allMods[cat].find(m => m.acronym === acronym);
        if (mod !== undefined) {
            return {
                category: cat,
                modData: mod
            };
        }
    }
    return {
        category: 'unknown',
        modData: Mod(acronym, acronym, false, [0, 1, 2, 3])
    };
}