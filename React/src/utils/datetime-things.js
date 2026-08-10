export function timeSpanToString(time) {
    const hours = Math.floor(time / 3600);
    const minutes = Math.floor((time - hours * 3600) / 60);
    const seconds = Math.floor(time - 3600 * hours - 60 * minutes);

    const hourString = hours < 10 ? '0' + hours : hours;
    const minuteString = minutes < 10 ? '0' + minutes : minutes;
    const secondString = seconds < 10 ? '0' + seconds : seconds;

    let finalString = '';
    if (hours > 0) {
        finalString += `${hourString}:`;
    }

    finalString += `${minuteString}:${secondString}`;
    return finalString;
}

export function dateStringFromDatetime(datetime) {
    const date = datetime.split('T')[0];
    const dateSplit = date.split('-');
    const year = dateSplit[0];
    const month = dateSplit[1];
    const day = dateSplit[2];
    
    return `${monthNumberToString(month)} ${day}, ${year}`;
}

export function YearMonthFromDateTime(datetime) {
    const date = datetime.split('T')[0];
    const dateSplit = date.split('-');
    const year = dateSplit[0];
    const month = dateSplit[1];

    return `${monthNumberToShortString(month)} ${year}`;
}

export function dateFromDateTime(datetime) {
    const date = new Date(datetime);
    const lang = navigator.languages[0] || navigator.language || navigator.browserLanguage;
    return date.toLocaleString(lang);
}

function monthNumberToString(month) {
    switch (month) {
        case '01':
            return 'January';
        case '02':
            return 'February';
        case '03':
            return 'March';
        case '04':
            return 'April';
        case '05':
            return 'May';
        case '06':
            return 'June';
        case '07':
            return 'July';
        case '08':
            return 'August';
        case '09':
            return 'September';
        case '10':
            return 'October';
        case '11':
            return 'November';
        case '12':
            return 'December';
    }
}

function monthNumberToShortString(month) {
    switch (month) {
        case '01':
            return 'Jan';
        case '02':
            return 'Feb';
        case '03':
            return 'Mar';
        case '04':
            return 'Apr';
        case '05':
            return 'May';
        case '06':
            return 'Jun';
        case '07':
            return 'Jul';
        case '08':
            return 'Aug';
        case '09':
            return 'Sep';
        case '10':
            return 'Oct';
        case '11':
            return 'Nov';
        case '12':
            return 'Dec';
    }
}