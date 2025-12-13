export const formatLasytPlayedDate = (lastPlayedDate?: string): string => {
    if (!lastPlayedDate) {
        return 'N/A';
    }
    const date = new Date(lastPlayedDate);
    const [month, day, year] = [date.getMonth() + 1, date.getDate(), date.getFullYear()];
    return `${year}/${month}/${day}`;
};
