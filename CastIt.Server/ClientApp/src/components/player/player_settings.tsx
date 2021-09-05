import {
    CircularProgress,
    createStyles,
    Dialog,
    DialogContent,
    Divider,
    FormControl,
    FormControlLabel,
    FormGroup,
    Grid,
    IconButton,
    InputLabel,
    makeStyles,
    Select,
    Slider,
    Switch,
    Theme,
    Typography,
} from '@material-ui/core';
import { Settings } from '@material-ui/icons';
import { Fragment, useContext, useEffect, useState } from 'react';
import {
    VideoScale,
    SubtitleBgColor,
    SubtitleFgColor,
    SubtitleFontScale,
    TextTrackFontStyle,
    TextTrackFontGenericFamily,
    AppLanguage,
} from '../../enums';
import { IServerAppSettings } from '../../models';
import { onPlayerSettingsChanged } from '../../services/castithub.service';
import translations from '../../services/translations';
import { getLanguageString, getLanguageEnum, TranslationContext } from '../../context/translations.context';
import { String } from 'typescript-string-operations';
import AppDialogTitle from '../dialogs/app_dialog_title';
import { useCastItHub } from '../../context/castit_hub.context';

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        formControl: {
            margin: theme.spacing(1),
            minWidth: 150,
        },
        gridItemMargin: {
            margin: theme.spacing(2, 1, 2, 1),
        },
    })
);

interface State {
    settings?: IServerAppSettings;
}

interface SelectOption {
    value: number;
    text: string;
}

function PlayerSettings() {
    const classes = useStyles();
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

    const subsBgColorOptions = mapEnum(SubtitleBgColor, (val) => ({ text: translations.default, value: val })).map(generateSelectOption);

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

    const subFontScaleOptions = mapEnum(SubtitleFontScale, (val) => ({ text: `${val} %`, value: val })).map(generateSelectOption);

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
        <Fragment>
            <IconButton onClick={handleOpenDialog}>
                <Settings fontSize="large" />
            </IconButton>
            <Dialog fullWidth={true} open={open} maxWidth="lg" onClose={handleCloseDialog}>
                <AppDialogTitle icon={<Settings />} title={translations.settings} close={handleCloseDialog} />
                <DialogContent>
                    <Grid container alignItems="flex-start" justifyContent="space-between">
                        <Grid item xs={12} md={6}>
                            <Grid container>
                                <Grid item xs={12} className={classes.gridItemMargin}>
                                    <Typography color="textSecondary">{translations.general}</Typography>
                                    <FormControl className={classes.formControl}>
                                        <InputLabel htmlFor="theme">{translations.theme}</InputLabel>
                                        <Select
                                            native
                                            value={0}
                                            inputProps={{
                                                name: 'theme',
                                                id: 'theme',
                                            }}
                                        >
                                            <option value={0}>{translations.dark}</option>
                                            <option value={1}>{translations.light}</option>
                                        </Select>
                                    </FormControl>
                                    <FormControl className={classes.formControl}>
                                        <InputLabel htmlFor="language">{translations.language}</InputLabel>
                                        <Select
                                            native
                                            value={getLanguageEnum(translationContext!.currentLanguage)}
                                            onChange={(e) => handleLanguageChange(e.target.value as number)}
                                            inputProps={{
                                                name: 'language',
                                                id: 'language',
                                            }}
                                        >
                                            {languageOptions}
                                        </Select>
                                    </FormControl>
                                    <FormControl className={classes.formControl}>
                                        <InputLabel htmlFor="video-scale">{translations.videoScale}</InputLabel>
                                        <Select
                                            native
                                            value={state.settings.videoScale}
                                            onChange={(e) => handleSettingsChange('videoScale', e.target.value)}
                                            inputProps={{
                                                name: 'video-scale',
                                                id: 'video-scale',
                                            }}
                                        >
                                            {videoScaleOptions}
                                        </Select>
                                    </FormControl>
                                </Grid>
                                <Grid item xs={12} className={classes.gridItemMargin}>
                                    <Typography color="textSecondary">{translations.options}</Typography>
                                    <Grid container>
                                        <Grid item xs={12} md={6}>
                                            <FormGroup row>
                                                <FormControlLabel
                                                    control={<Switch color="primary" checked={true} />}
                                                    label={translations.showFileDetails}
                                                />
                                                <FormControlLabel
                                                    control={
                                                        <Switch
                                                            onChange={(e, checked) =>
                                                                handleSettingsChange('startFilesFromTheStart', checked)
                                                            }
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
                                    </Grid>
                                </Grid>
                            </Grid>
                        </Grid>
                        <Divider orientation="vertical" flexItem />
                        <Grid item xs={12} md={5}>
                            <Grid container>
                                <Grid item xs={12} className={classes.gridItemMargin}>
                                    <Typography color="textSecondary">{translations.subtitles}</Typography>
                                    <FormControl className={classes.formControl}>
                                        <InputLabel htmlFor="font-color">{translations.fontColor}</InputLabel>
                                        <Select
                                            native
                                            value={state.settings!.currentSubtitleFgColor}
                                            onChange={(e) => handleSettingsChange('currentSubtitleFgColor', e.target.value)}
                                            inputProps={{
                                                name: 'font-color',
                                                id: 'font-color',
                                            }}
                                        >
                                            {subsFgColorOptions}
                                        </Select>
                                    </FormControl>
                                    <FormControl className={classes.formControl}>
                                        <InputLabel htmlFor="font-bg">{translations.fontBackground}</InputLabel>
                                        <Select
                                            native
                                            onChange={(e) => handleSettingsChange('currentSubtitleBgColor', e.target.value)}
                                            value={state.settings!.currentSubtitleBgColor}
                                            inputProps={{
                                                name: 'font-bg',
                                                id: 'font-bg',
                                            }}
                                        >
                                            {subsBgColorOptions}
                                        </Select>
                                    </FormControl>
                                    <FormControl className={classes.formControl}>
                                        <InputLabel htmlFor="font-style">{translations.fontStyle}</InputLabel>
                                        <Select
                                            native
                                            value={state.settings!.currentSubtitleFontStyle}
                                            onChange={(e) => handleSettingsChange('currentSubtitleFontStyle', e.target.value)}
                                            inputProps={{
                                                name: 'font-style',
                                                id: 'font-style',
                                            }}
                                        >
                                            {subFontStyleOptions}
                                        </Select>
                                    </FormControl>
                                    <FormControl className={classes.formControl}>
                                        <InputLabel htmlFor="font-family">{translations.fontFamily}</InputLabel>
                                        <Select
                                            native
                                            onChange={(e) => handleSettingsChange('currentSubtitleFontFamily', e.target.value)}
                                            value={state.settings!.currentSubtitleFontFamily}
                                            inputProps={{
                                                name: 'font-family',
                                                id: 'font-family',
                                            }}
                                        >
                                            {subFontGenericFamilyOptions}
                                        </Select>
                                    </FormControl>
                                    <FormControl className={classes.formControl}>
                                        <InputLabel htmlFor="font-scale">{translations.fontScale}</InputLabel>
                                        <Select
                                            native
                                            onChange={(e) => handleSettingsChange('currentSubtitleFontScale', e.target.value)}
                                            value={state.settings!.currentSubtitleFontScale}
                                            inputProps={{
                                                name: 'font-scale',
                                                id: 'font-scale',
                                            }}
                                        >
                                            {subFontScaleOptions}
                                        </Select>
                                    </FormControl>
                                </Grid>
                                <Grid item xs={12} className={classes.gridItemMargin}>
                                    <Typography id="subtitle-delay-slider" gutterBottom>
                                        {String.Format(translations.subtitleDelayXSeconds, state.settings.subtitleDelayInSeconds)}
                                    </Typography>
                                    <Slider
                                        min={-10}
                                        max={10}
                                        step={0.1}
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
                                </Grid>
                            </Grid>
                        </Grid>
                    </Grid>
                </DialogContent>
            </Dialog>
        </Fragment>
    );
}

export default PlayerSettings;
