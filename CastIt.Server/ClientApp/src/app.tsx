import { createTheme, ThemeProvider, StyledEngineProvider } from '@mui/material';
import { green, purple } from '@mui/material/colors';
import CssBaseline from '@mui/material/CssBaseline';
import { BrowserRouter } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import { AppRoutes } from './routes';
import { Suspense } from 'react';
import ServerMessage from './components/server_message';
import { TranslationContextProvider } from './context/translations.context';
import { CastItHubContextProvider } from './context/castit_hub.context';
import Loading from './components/loading';
import translations from './services/translations';
import Player from './components/player/player';

const theme = createTheme({
    palette: {
        mode: 'dark',
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
                <StyledEngineProvider injectFirst>
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
                </StyledEngineProvider>
            </BrowserRouter>
        </SnackbarProvider>
    );
}

export default App;
