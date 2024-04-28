import {
    CircularProgress,
    Dialog,
    DialogContent,
    Divider,
    FormControl,
    FormControlLabel,
    FormGroup,
    Grid,
    IconButton,
    InputLabel,
    Select,
    Slider,
    Switch,
    Typography,
} from '@mui/material';
import { styled } from '@mui/material/styles';
import { Settings } from '@mui/icons-material';
import { useContext, useEffect, useState } from 'react';
import {
    VideoScale,
    SubtitleBgColor,
    SubtitleFgColor,
    SubtitleFontScale,
    TextTrackFontStyle,
    TextTrackFontGenericFamily,
    AppLanguage,
    WebVideoQuality,
} from '../../enums';
import { IServerAppSettings } from '../../models';
import { onPlayerSettingsChanged } from '../../services/castithub.service';
import translations from '../../services/translations';
import { getLanguageString, getLanguageEnum, TranslationContext } from '../../context/translations.context';
import { String } from 'typescript-string-operations';
import AppDialogTitle from '../dialogs/app_dialog_title';
import { useCastItHub } from '../../context/castit_hub.context';

const StyledFormControl = styled(FormControl)(({ theme }) => ({
    margin: theme.spacing(1),
    minWidth: 150,
}));

const StyledGeneralGrid = styled(Grid)(({ theme }) => ({
    margin: theme.spacing(1),
}));

const StyledGridItem = styled(Grid)(({ theme }) => ({
    margin: theme.spacing(2, 0, 0, 0),
}));

interface State {
    settings?: IServerAppSettings;
}

interface SelectOption {
    value: number;
    text: string;
}

function PlayerSettings() {
    const [open, setOpen] = useState(false);
    const [state, setState] = useState<State>({});
    const [translationContext, translationState] = useContext(TranslationContext);
    const castItHub = useCastItHub();

    useEffect(() => {
        const onPlayerSettingsChangedSubscription = onPlayerSettingsChanged.subscribe((settings) => {
            if (!settings) {
                return;
            }
            setState({ settings: settings });
        });
        return () => {
            onPlayerSettingsChangedSubscription.unsubscribe();
        };
    }, []);

    if (!state.settings) {
        return <CircularProgress />;
    }

    const mapEnum = (enumerable: any, fn: (number: number) => SelectOption): SelectOption[] => {
        // get all the members of the enum
        const enumMembers: any[] = Object.keys(enumerable).map((key) => enumerable[key]);

        // we are only interested in the numeric identifiers as these represent the values
        const enumValues: number[] = enumMembers.filter((v) => typeof v === 'number');

        // now map through the enum values
        return enumValues.map((m) => fn(m));
    };

    const generateSelectOption = (option: SelectOption) => (
        <option key={option.value} value={option.value}>
            {option.text}
        </option>
    );

    const languageOptions = mapEnum(AppLanguage, (val) => {
        switch (val) {
            case AppLanguage.english:
                return { text: translations.english, value: val };
            default:
                return { text: translations.spanish, value: val };
        }
    }).map(generateSelectOption);

    const videoScaleOptions = mapEnum(VideoScale, (val) => {
        switch (val) {
            case VideoScale.fullHd:
                return { text: translations.fullHd, value: val };
            case VideoScale.hd:
                return { text: translations.hd, value: val };
            default:
                return { text: translations.original, value: val };
        }
    }).map(generateSelectOption);

    const supportedWebVideoQualities = mapEnum(WebVideoQuality, (val) => {
        return { text: `${val}p`, value: val };
    }).map(generateSelectOption);

    const subsBgColorOptions = mapEnum(SubtitleBgColor, (val) => ({
        text: translations.default,
        value: val,
    })).map(generateSelectOption);

    const subsFgColorOptions = mapEnum(SubtitleFgColor, (val) => {
        switch (val) {
            case SubtitleFgColor.white:
                return { text: translations.white, value: val };
            default:
                return { text: translations.yellow, value: val };
        }
    }).map(generateSelectOption);

    const subFontStyleOptions = mapEnum(TextTrackFontStyle, (val) => {
        switch (val) {
            case TextTrackFontStyle.bold:
                return { text: translations.bold, value: val };
            case TextTrackFontStyle.boldItalic:
                return { text: translations.boldAndItalic, value: val };
            case TextTrackFontStyle.italic:
                return { text: translations.italic, value: val };
            default:
                return { text: translations.normal, value: val };
        }
    }).map(generateSelectOption);

    const subFontGenericFamilyOptions = mapEnum(TextTrackFontGenericFamily, (val) => {
        switch (val) {
            case TextTrackFontGenericFamily.cursive:
                return { text: 'Cursive', value: val };
            case TextTrackFontGenericFamily.monospacedSansSerif:
                return { text: 'Monospaced Sans Serif', value: val };
            case TextTrackFontGenericFamily.monospacedSerif:
                return { text: 'Monospaced Serif', value: val };
            case TextTrackFontGenericFamily.sansSerif:
                return { text: 'Sans Serif', value: val };
            case TextTrackFontGenericFamily.serif:
                return { text: 'Serif', value: val };
            case TextTrackFontGenericFamily.smallCapitals:
                return { text: 'Small Capitals', value: val };
            default:
                return { text: 'Casual', value: val };
        }
    }).map(generateSelectOption);

    const subFontScaleOptions = mapEnum(SubtitleFontScale, (val) => ({
        text: `${val} %`,
        value: val,
    })).map(generateSelectOption);

    const handleOpenDialog = (): void => {
        setOpen(true);
    };

    const handleCloseDialog = (): void => {
        setOpen(false);
    };

    const handleLanguageChange = (lang: number): void => {
        translationState!({ currentLanguage: getLanguageString(lang) });
    };

    const handleSubtitlesDelayChange = (val: number) => {
        const updatedSettings = { ...state.settings! };
        updatedSettings.subtitleDelayInSeconds = val;
        setState({
            settings: updatedSettings,
        });
    };

    const handleSettingsChange = async (key: keyof IServerAppSettings, val: any): Promise<void> => {
        let updatedSettings: IServerAppSettings = { ...state.settings! };
        const type = typeof updatedSettings[key];
        switch (type) {
            case 'boolean':
                updatedSettings = { ...updatedSettings, [key]: Boolean(val) };
                break;
            case 'number':
                updatedSettings = { ...updatedSettings, [key]: Number(val) };
                break;
            default:
                console.error('Settings case not handled');
                break;
        }

        await castItHub.connection.updateSettings(updatedSettings);
    };

    return (
        <>
            <IconButton onClick={handleOpenDialog} size="large">
                <Settings fontSize="large" />
            </IconButton>
            <Dialog fullWidth={true} open={open} maxWidth="lg" onClose={handleCloseDialog}>
                <AppDialogTitle icon={<Settings />} title={translations.settings} close={handleCloseDialog} />
                <DialogContent>
                    <Grid container alignItems="flex-start" justifyContent="space-between">
                        <Grid item xs={12} md={6}>
                            <StyledGridItem item xs={12}>
                                <Typography color="textSecondary">{translations.general}</Typography>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.theme}</InputLabel>
                                    <Select
                                        native
                                        label={translations.theme}
                                        value={0}
                                        inputProps={{
                                            name: 'theme',
                                            id: 'theme',
                                        }}
                                    >
                                        <option value={0}>{translations.dark}</option>
                                        <option value={1}>{translations.light}</option>
                                    </Select>
                                </StyledFormControl>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.language}</InputLabel>
                                    <Select
                                        native
                                        label={translations.language}
                                        value={getLanguageEnum(translationContext!.currentLanguage)}
                                        onChange={(e) => handleLanguageChange(e.target.value as number)}
                                        inputProps={{
                                            name: 'language',
                                            id: 'language',
                                        }}
                                    >
                                        {languageOptions}
                                    </Select>
                                </StyledFormControl>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.videoScale}</InputLabel>
                                    <Select
                                        native
                                        label={translations.videoScale}
                                        value={state.settings.videoScale}
                                        onChange={(e) => handleSettingsChange('videoScale', e.target.value)}
                                        inputProps={{
                                            name: 'video-scale',
                                            id: 'video-scale',
                                        }}
                                    >
                                        {videoScaleOptions}
                                    </Select>
                                </StyledFormControl>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.webVideoQuality}</InputLabel>
                                    <Select
                                        native
                                        label={translations.webVideoQuality}
                                        value={state.settings.webVideoQuality}
                                        onChange={(e) => handleSettingsChange('webVideoQuality', e.target.value)}
                                        inputProps={{
                                            name: 'supported-video-qualities',
                                            id: 'supported-video-qualities',
                                        }}
                                    >
                                        {supportedWebVideoQualities}
                                    </Select>
                                </StyledFormControl>
                            </StyledGridItem>
                            <StyledGridItem item xs={12}>
                                <Typography color="textSecondary">{translations.options}</Typography>
                                <StyledGeneralGrid container>
                                    <Grid item xs={12} md={6}>
                                        <FormGroup row>
                                            <FormControlLabel
                                                control={<Switch color="primary" checked={true} />}
                                                label={translations.showFileDetails}
                                            />
                                            <FormControlLabel
                                                control={
                                                    <Switch
                                                        onChange={(e, checked) => handleSettingsChange('startFilesFromTheStart', checked)}
                                                        color="primary"
                                                        checked={state.settings.startFilesFromTheStart}
                                                    />
                                                }
                                                label={translations.startFilesFromTheStart}
                                            />
                                            <FormControlLabel
                                                control={
                                                    <Switch
                                                        onChange={(e, checked) =>
                                                            handleSettingsChange('playNextFileAutomatically', checked)
                                                        }
                                                        color="primary"
                                                        checked={state.settings.playNextFileAutomatically}
                                                    />
                                                }
                                                label={translations.playNextFileAutomatically}
                                            />
                                        </FormGroup>
                                    </Grid>
                                    <Grid item xs={12} md={6}>
                                        <FormGroup row>
                                            <FormControlLabel
                                                control={
                                                    <Switch
                                                        onChange={(e, checked) => handleSettingsChange('forceVideoTranscode', checked)}
                                                        color="primary"
                                                        checked={state.settings.forceVideoTranscode}
                                                    />
                                                }
                                                label={translations.forceVideoTranscode}
                                            />
                                            <FormControlLabel
                                                control={
                                                    <Switch
                                                        onChange={(e, checked) => handleSettingsChange('forceAudioTranscode', checked)}
                                                        color="primary"
                                                        checked={state.settings.forceAudioTranscode}
                                                    />
                                                }
                                                label={translations.forceAudioTranscode}
                                            />
                                            <FormControlLabel
                                                control={
                                                    <Switch
                                                        onChange={(e, checked) =>
                                                            handleSettingsChange('enableHardwareAcceleration', checked)
                                                        }
                                                        color="primary"
                                                        checked={state.settings.enableHardwareAcceleration}
                                                    />
                                                }
                                                label={translations.enableHardwareAcceleration}
                                            />
                                        </FormGroup>
                                    </Grid>
                                </StyledGeneralGrid>
                            </StyledGridItem>
                        </Grid>
                        <Divider orientation="vertical" flexItem />
                        <Grid item xs={12} md={5}>
                            <StyledGridItem item xs={12}>
                                <Typography color="textSecondary">{translations.subtitles}</Typography>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.fontColor}</InputLabel>
                                    <Select
                                        native
                                        label={translations.fontColor}
                                        value={state.settings!.currentSubtitleFgColor}
                                        onChange={(e) => handleSettingsChange('currentSubtitleFgColor', e.target.value)}
                                        inputProps={{
                                            name: 'font-color',
                                            id: 'font-color',
                                        }}
                                    >
                                        {subsFgColorOptions}
                                    </Select>
                                </StyledFormControl>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.fontBackground}</InputLabel>
                                    <Select
                                        native
                                        label={translations.fontBackground}
                                        onChange={(e) => handleSettingsChange('currentSubtitleBgColor', e.target.value)}
                                        value={state.settings!.currentSubtitleBgColor}
                                        inputProps={{
                                            name: 'font-bg',
                                            id: 'font-bg',
                                        }}
                                    >
                                        {subsBgColorOptions}
                                    </Select>
                                </StyledFormControl>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.fontStyle}</InputLabel>
                                    <Select
                                        native
                                        label={translations.fontStyle}
                                        value={state.settings!.currentSubtitleFontStyle}
                                        onChange={(e) => handleSettingsChange('currentSubtitleFontStyle', e.target.value)}
                                        inputProps={{
                                            name: 'font-style',
                                            id: 'font-style',
                                        }}
                                    >
                                        {subFontStyleOptions}
                                    </Select>
                                </StyledFormControl>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.fontFamily}</InputLabel>
                                    <Select
                                        native
                                        label={translations.fontFamily}
                                        onChange={(e) => handleSettingsChange('currentSubtitleFontFamily', e.target.value)}
                                        value={state.settings!.currentSubtitleFontFamily}
                                        inputProps={{
                                            name: 'font-family',
                                            id: 'font-family',
                                        }}
                                    >
                                        {subFontGenericFamilyOptions}
                                    </Select>
                                </StyledFormControl>
                                <StyledFormControl size="small">
                                    <InputLabel>{translations.fontScale}</InputLabel>
                                    <Select
                                        native
                                        label={translations.fontScale}
                                        onChange={(e) => handleSettingsChange('currentSubtitleFontScale', e.target.value)}
                                        value={state.settings!.currentSubtitleFontScale}
                                        inputProps={{
                                            name: 'font-scale',
                                            id: 'font-scale',
                                        }}
                                    >
                                        {subFontScaleOptions}
                                    </Select>
                                </StyledFormControl>
                            </StyledGridItem>
                            <StyledGeneralGrid item xs={12}>
                                <Typography id="subtitle-delay-slider" gutterBottom color="textSecondary">
                                    {String.Format(translations.subtitleDelayXSeconds, state.settings.subtitleDelayInSeconds)}
                                </Typography>
                                <Slider
                                    min={-10}
                                    max={10}
                                    step={0.1}
                                    size="small"
                                    onChangeCommitted={(e, val) => handleSettingsChange('subtitleDelayInSeconds', val)}
                                    onChange={(e, val) => handleSubtitlesDelayChange(val as number)}
                                    value={state.settings.subtitleDelayInSeconds}
                                    aria-labelledby="subtitle-delay-slider"
                                />
                                <FormGroup row>
                                    <FormControlLabel
                                        control={
                                            <Switch
                                                onChange={(e, checked) =>
                                                    handleSettingsChange('loadFirstSubtitleFoundAutomatically', checked)
                                                }
                                                color="primary"
                                                checked={state.settings.loadFirstSubtitleFoundAutomatically}
                                            />
                                        }
                                        label={translations.loadFirstSubtitleFoundAutomatically}
                                    />
                                </FormGroup>
                            </StyledGeneralGrid>
                        </Grid>
                    </Grid>
                </DialogContent>
            </Dialog>
        </>
    );
}

export default PlayerSettings;
