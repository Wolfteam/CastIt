import { createTheme, ThemeProvider } from '@material-ui/core';
import { green, purple } from '@material-ui/core/colors';
import CssBaseline from '@material-ui/core/CssBaseline';
import { BrowserRouter } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import { AppRoutes, PlayerRoutes } from './routes';
import { Suspense } from 'react';
import ServerMessage from './components/server_message';
import { TranslationContextProvider } from './context/translations.context';
import { CastItHubContextProvider } from './context/castit_hub.context';
import Loading from './components/loading';
import translations from './services/translations';
import Player from './components/player/player';

const theme = createTheme({
    palette: {
        type: 'dark',
        primary: {
            main: purple[500],
        },
        secondary: {
            main: green[500],
        },
    },
});

function App() {
    return (
        <SnackbarProvider
            autoHideDuration={3000}
            anchorOrigin={{
                vertical: 'bottom',
                horizontal: 'right',
            }}
        >
            <BrowserRouter>
                <ThemeProvider theme={theme}>
                    <CssBaseline />
                    <TranslationContextProvider>
                        <CastItHubContextProvider>
                            <ServerMessage>
                                <Suspense fallback={<Loading message={translations.loading + '...'} />}>
                                    <AppRoutes />
                                    <Player />
                                </Suspense>
                            </ServerMessage>
                        </CastItHubContextProvider>
                    </TranslationContextProvider>
                </ThemeProvider>
            </BrowserRouter>
        </SnackbarProvider>
    );
}

export default App;
