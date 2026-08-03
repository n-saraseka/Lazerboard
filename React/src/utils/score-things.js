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