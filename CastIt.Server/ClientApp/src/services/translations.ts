import LocalizedStrings, { LocalizedStringsMethods } from 'react-localization';
import { AppMessage } from '../enums';

interface IAppMessageTranslations {
    unknownErrorOccurred: string;
    invalidRequest: string;
    notFound: string;
    playListNotFound: string;
    unknownErrorLoadingFile: string;
    fileNotFound: string;
    fileIsAlreadyBeingPlayed: string;
    fileNotSupported: string;
    filesAreNotValid: string;
    noFilesToBeAdded: string;
    urlNotSupported: string;
    urlCouldntBeParsed: string;
    oneOrMoreFilesAreNotReadyYet: string;
    noDevicesFound: string;
    noInternetConnection: string;
    connectionToDeviceIsStillInProgress: string;
    ffmpegError: string;
    serverIsClosing: string;
    ffmpegExecutableNotFound: string;
}

interface ITranslations {
    ok: string;
    cancel: string;
    nothingFound: string;
    invalidPlayList: string;
    play: string;
    playFromTheStart: string;
    addFolder: string;
    addFiles: string;
    addUrl: string;
    remove: string;
    removeAllMissing: string;
    refresh: string;
    refreshing: string;
    search: string;
    audio: string;
    quality: string;
    rename: string;
    delete: string;
    playList: string;
    addNewPlayList: string;
    loop: string;
    disableLoop: string;
    shuffle: string;
    language: string;
    english: string;
    spanish: string;
    settings: string;
    options: string;
    showFileDetails: string;
    startFilesFromTheStart: string;
    playNextFileAutomatically: string;
    forceVideoTranscode: string;
    forceAudioTranscode: string;
    enableHardwareAcceleration: string;
    theme: string;
    dark: string;
    light: string;
    subtitles: string;
    fontColor: string;
    fontBackground: string;
    fontStyle: string;
    fontFamily: string;
    fontScale: string;
    subtitleDelayXSeconds: string;
    loadFirstSubtitleFoundAutomatically: string;
    default: string;
    white: string;
    yellow: string;
    videoScale: string;
    fullHd: string;
    hd: string;
    original: string;
    bold: string;
    boldAndItalic: string;
    italic: string;
    normal: string;
    fieldIsNotValid: string;
    devices: string;
    general: string;
    includeSubFolders: string;
    onlyVideo: string;
    load: string;
    path: string;
    loading: string;
    connecting: string;
    connectionFailedMsg: string;
    retry: string;
    sort: string;
    copyPath: string;
    errorCodes: IAppMessageTranslations;
}

interface IStrings extends LocalizedStringsMethods, ITranslations {}

const enTrans: ITranslations = {
    ok: 'Ok',
    cancel: 'Cancel',
    nothingFound: 'Nothing found',
    invalidPlayList: 'Invalid playlist',
    play: 'Play',
    playFromTheStart: 'Play from the start',
    addFolder: 'Add folder',
    addFiles: 'Add files',
    addUrl: 'Add url',
    remove: 'Remove',
    removeAllMissing: 'Remove all missing',
    refresh: 'Refresh',
    refreshing: 'Refreshing',
    search: 'Search',
    audio: 'Audio',
    quality: 'Quality',
    rename: 'Rename',
    delete: 'Delete',
    playList: 'PlayList',
    addNewPlayList: 'Add new playlist',
    loop: 'Loop',
    disableLoop: 'Disable loop',
    shuffle: 'Shuffle',
    language: 'Language',
    english: 'English',
    spanish: 'Spanish',
    settings: 'Settings',
    options: 'Options',
    showFileDetails: 'Show file details',
    startFilesFromTheStart: 'Start files from the start',
    playNextFileAutomatically: 'Play next file automatically',
    forceVideoTranscode: 'Force video transcode',
    forceAudioTranscode: 'Force audio transcode',
    enableHardwareAcceleration: 'Enable hardware acceleration',
    theme: 'Theme',
    dark: 'Dark',
    light: 'Light',
    subtitles: 'Subtitles',
    fontColor: 'Font color',
    fontBackground: 'Font background',
    fontStyle: 'Font style',
    fontFamily: 'Font family',
    fontScale: 'Font scale',
    subtitleDelayXSeconds: 'Subtitle delay ({0} second(s))',
    loadFirstSubtitleFoundAutomatically: 'Load first subtitle found automatically',
    default: 'Default',
    white: 'White',
    yellow: 'Yellow',
    videoScale: 'Video scale',
    fullHd: 'Full HD',
    hd: 'HD',
    original: 'Original',
    bold: 'Bold',
    boldAndItalic: 'Bold & Italic',
    italic: 'Italic',
    normal: 'Normal',
    fieldIsNotValid: 'Field is not valid',
    devices: 'Devices',
    general: 'General',
    includeSubFolders: 'Include subfolders',
    onlyVideo: 'Only video',
    load: 'Load',
    path: 'Path',
    loading: 'Loading',
    connecting: 'Connecting',
    connectionFailedMsg: 'Connection failed. The server may not be running',
    retry: 'Retry',
    sort: 'Sort',
    copyPath: 'Copy path',
    errorCodes: {
        unknownErrorOccurred: 'Unknown error occurred',
        invalidRequest: 'Invalid request',
        notFound: 'The resource you were looking for does not exist',
        playListNotFound: 'PlayList not found',
        unknownErrorLoadingFile: 'Unknown error occurred while trying to play file',
        fileNotFound: 'File not found',
        fileIsAlreadyBeingPlayed: 'File is already being played',
        fileNotSupported: 'One or more files are not supported',
        filesAreNotValid: 'One or more files are not valid',
        noFilesToBeAdded: 'No files to be added',
        urlNotSupported: 'Url is not supported',
        urlCouldntBeParsed: 'Url could not be parsed',
        oneOrMoreFilesAreNotReadyYet: 'One or more files are not ready yet',
        noDevicesFound: 'No devices found',
        noInternetConnection: 'No internet connection',
        connectionToDeviceIsStillInProgress: 'Connection to device is still in progress',
        ffmpegError: 'Ffmpeg error',
        serverIsClosing: 'Server is closing',
        ffmpegExecutableNotFound: 'Ffmpeg executable not found',
    },
};

const esTrans: ITranslations = {
    ok: 'Ok',
    cancel: 'Cancelar',
    nothingFound: 'Nada encontrado',
    invalidPlayList: 'Lista de reproducción inválida',
    play: 'Reproducir',
    playFromTheStart: 'Reproducir desde el principio',
    addFolder: 'Agregar carpeta',
    addFiles: 'Agregar archivos',
    addUrl: 'Agregar url',
    remove: 'Remover',
    removeAllMissing: 'Remover todos los faltantes',
    refresh: 'Refrescar',
    refreshing: 'Refrescando',
    search: 'Buscar',
    audio: 'Audio',
    quality: 'Calidad',
    rename: 'Renombrar',
    delete: 'Borrar',
    playList: 'Lista de reproducción',
    addNewPlayList: 'Agregar nueva lista de reproducción',
    loop: 'Bucle',
    disableLoop: 'Deshabilitar bucle',
    shuffle: 'Aleatorio',
    language: 'Idioma',
    english: 'Inglés',
    spanish: 'Español',
    settings: 'Ajustes',
    options: 'Opciones',
    showFileDetails: 'Mostrar detalles del archivo',
    startFilesFromTheStart: 'Iniciar archivos desde el comienzo',
    playNextFileAutomatically: 'Reproducir el siguiente archivo automáticamente',
    forceVideoTranscode: 'Forzar el transcode de video',
    forceAudioTranscode: 'Forzar el transcode de audio',
    enableHardwareAcceleration: 'Habilitar la acceleración por hardware',
    theme: 'Tema',
    dark: 'Oscuro',
    light: 'Ligero',
    subtitles: 'Subtítulos',
    fontColor: 'Color de la fuente',
    fontBackground: 'Color de fondo de la fuente',
    fontStyle: 'Estilo de la fuente',
    fontFamily: 'Familia de la fuente',
    fontScale: 'Escala de la fuente',
    subtitleDelayXSeconds: 'Retraso de los subtitulos ({0} segundo(s))',
    loadFirstSubtitleFoundAutomatically: 'Cargar el primer subtítulo encontrado automáticamente',
    default: 'Por defecto',
    white: 'Blanco',
    yellow: 'Amarillo',
    videoScale: 'Escala del video',
    fullHd: 'Full HD',
    hd: 'HD',
    original: 'Original',
    bold: 'Negritas',
    boldAndItalic: 'Negritas & Itálicas',
    italic: 'Itálicas',
    normal: 'Normal',
    fieldIsNotValid: 'El campo no es válido',
    devices: 'Dispositivos',
    general: 'General',
    includeSubFolders: 'Incluir subcarpetas',
    onlyVideo: 'Solo video',
    load: 'Cargar',
    path: 'Ruta',
    loading: 'Cargando',
    connecting: 'Conectando',
    connectionFailedMsg: 'La conexión falló. El servidor podria no estar ejecutandose',
    retry: 'Reintentar',
    sort: 'Ordenar',
    copyPath: 'Copiar ruta',
    errorCodes: {
        unknownErrorOccurred: 'Un error inesperado ha ocurrido',
        invalidRequest: 'Petición no válida',
        notFound: 'El recurso que estabas buscando no existe',
        playListNotFound: 'Lista de reproducción no encontrada',
        unknownErrorLoadingFile: 'Un error inesperado ha ocurrido al intentar reproducir el archivo',
        fileNotFound: 'Archivo no encontrado',
        fileIsAlreadyBeingPlayed: 'El archivo ya está siendo reproducido',
        fileNotSupported: 'Uno o mas archivos no están soportados',
        filesAreNotValid: 'Uno o mas archivos no son válidos',
        noFilesToBeAdded: 'No hay archivos para agregar',
        urlNotSupported: 'La ruta no está soportada',
        urlCouldntBeParsed: 'La ruta no pudo ser parseada',
        oneOrMoreFilesAreNotReadyYet: 'Uno o más archivos no están listos',
        noDevicesFound: 'No se encontraron dispositivos',
        noInternetConnection: 'No hay conexión a internet',
        connectionToDeviceIsStillInProgress: 'Una conexión a un dispositivo está en progreso',
        ffmpegError: 'Error de Ffmpeg',
        serverIsClosing: 'El servidor está cerrando',
        ffmpegExecutableNotFound: 'El ejecutable de ffmpeg no fue encontrado',
    },
};

const translations: IStrings = new LocalizedStrings({
    en: enTrans,
    es: esTrans,
});

export const getErrorCodeTranslation = (errorMsgId: AppMessage): string => {
    let message = translations.errorCodes.unknownErrorOccurred;

    const key = AppMessage[errorMsgId];
    if (key in translations.errorCodes) {
        const theKey = key as keyof IAppMessageTranslations;
        message = translations.errorCodes[theKey];
    } else {
        console.warn(`Key = ${errorMsgId} is not being handled`);
    }

    return message;
};

export default translations;
