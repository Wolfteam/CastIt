import { IconButton, Divider, ListItemText, Menu, MenuItem } from '@mui/material';
import { styled } from '@mui/material/styles';
import { Audiotrack, HighQuality, Search, Subtitles, CheckTwoTone } from '@mui/icons-material';
import MenuIcon from '@mui/icons-material/Menu';
import { Fragment, JSX, useEffect, useState } from 'react';
import PopupState, { bindTrigger, bindMenu, InjectedProps } from 'material-ui-popup-state';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import { IFileItemOptionsResponseDto } from '../../models';
import translations from '../../services/translations';
import { useCastItHub } from '../../context/castit_hub.context';
import AddSubtitlesDialog from '../dialogs/add_subtitles_dialog';

const StyledListItemText = styled(ListItemText)({
    marginLeft: 10,
});

interface State {
    contentIsBeingPlayed: boolean;
    selectedAudioIndex: number;
    audios: IFileItemOptionsResponseDto[];
    selectedSubtitleIndex: number;
    subtitles: IFileItemOptionsResponseDto[];
    selectedQualityIndex: number;
    qualities: IFileItemOptionsResponseDto[];
}

const initialState: State = {
    contentIsBeingPlayed: false,
    selectedAudioIndex: 0,
    audios: [],
    selectedSubtitleIndex: 0,
    subtitles: [],
    selectedQualityIndex: 0,
    qualities: [],
};

const loadSubsKey = -999;

function PlayerFileOptions() {
    const [state, setState] = useState(initialState);
    const castItHub = useCastItHub();
    const [showAddSubtitles, setshowAddSubtitles] = useState(false);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status) {
                return;
            }

            if (!status.playedFile) {
                setState(initialState);
                return;
            }

            setState({
                contentIsBeingPlayed: true,
                selectedAudioIndex: status.playedFile.currentFileAudioStreamIndex,
                audios: status.playedFile.currentFileAudios,
                selectedSubtitleIndex: status.playedFile.currentFileSubTitleStreamIndex,
                subtitles: status.playedFile.currentFileSubTitles,
                selectedQualityIndex: status.playedFile.currentFileQuality,
                qualities: status.playedFile.currentFileQualities,
            });
        });
        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, []);

    if (!state.contentIsBeingPlayed) {
        return null;
    }

    const handleOptionChange = async (id: number, popupState: InjectedProps): Promise<void> => {
        popupState.close();

        if (id === loadSubsKey) {
            setshowAddSubtitles(true);
            return;
        }

        const options = [...state.audios, ...state.subtitles, ...state.qualities];
        const selected = options.find((op) => op.id === id);
        if (!selected) {
            return;
        }
        await castItHub.connection.setFileOptions(id, selected.isAudio, selected.isSubTitle, selected.isQuality);
    };

    const handleSubtitlesSet = async (path: string | null): Promise<void> => {
        if (path) {
            await castItHub.connection.setFileSubtitlesFromPath(path);
        }

        setshowAddSubtitles(false);
    };

    const buildSubMenuItem = (
        key: string,
        text: string,
        isSelected: boolean,
        isEnabled: boolean,
        popupState: InjectedProps,
        icon: JSX.Element | null = null
    ): JSX.Element => (
        <MenuItem key={key} sx={{paddingLeft: 5}} disabled={!isEnabled} onClick={() => handleOptionChange(Number(key), popupState)}>
            {isSelected ? <CheckTwoTone /> : icon}
            <StyledListItemText primary={text} />
        </MenuItem>
    );

    const buildSubMenuItemFromOption = (option: IFileItemOptionsResponseDto, popupState: InjectedProps): JSX.Element =>
        buildSubMenuItem(`${option.id}`, option.text, option.isSelected, option.isEnabled, popupState);

    const audios = (popupState: InjectedProps): JSX.Element[] => {
        const items = state.audios.map((audio) => buildSubMenuItemFromOption(audio, popupState));

        if (items.length === 0) {
            items.push(buildSubMenuItem('na-audio', 'N/A', true, false, popupState));
        }
        return items;
    };

    const subtitles = (popupState: InjectedProps): JSX.Element[] =>
        [buildSubMenuItem(`${loadSubsKey}`, translations.load, false, true, popupState, <Search />)].concat(
            state.subtitles.map((sub) => buildSubMenuItemFromOption(sub, popupState))
        );

    const qualities = (popupState: InjectedProps): JSX.Element[] => {
        const items = state.qualities.map((quality) => buildSubMenuItemFromOption(quality, popupState));

        if (items.length === 0) {
            items.push(buildSubMenuItem('na-quality', 'N/A', true, false, popupState));
        }
        return items;
    };

    return (
        <PopupState variant="popover">
            {(popupState) => (
                <Fragment>
                    <IconButton color="inherit" {...bindTrigger(popupState)} size="large">
                        <MenuIcon fontSize="large" />
                    </IconButton>
                    <AddSubtitlesDialog isOpen={showAddSubtitles} onClose={handleSubtitlesSet} />
                    <Menu {...bindMenu(popupState)}>
                        <MenuItem disabled>
                            <Subtitles />
                            <StyledListItemText primary={translations.subtitles} />
                        </MenuItem>
                        {subtitles(popupState)}
                        <Divider />
                        <MenuItem disabled>
                            <Audiotrack />
                            <StyledListItemText primary={translations.audio} />
                        </MenuItem>
                        {audios(popupState)}
                        <Divider />
                        <MenuItem disabled>
                            <HighQuality />
                            <StyledListItemText primary={translations.quality} />
                        </MenuItem>
                        {qualities(popupState)}
                    </Menu>
                </Fragment>
            )}
        </PopupState>
    );
}

export default PlayerFileOptions;
