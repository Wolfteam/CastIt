import { CircularProgress, Container, createTheme, Grid, ThemeProvider } from '@material-ui/core';
import { green, purple } from '@material-ui/core/colors';
import CssBaseline from '@material-ui/core/CssBaseline';
import { BrowserRouter } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import { AppRoutes, PlayerRoutes } from './routes';
import { Suspense } from 'react';
import ServerMessage from './components/server_message';
import { TranslationContextProvider } from './context/translations.context';
import { CastItHubContextProvider } from './context/castit_hub.context';

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
    const loading = (
        <Container>
            <Grid container justifyContent="center" alignItems="center" direction="column" style={{ minHeight: '100vh' }}>
                <Grid item xs={12}>
                    <CircularProgress />
                </Grid>
            </Grid>
        </Container>
    );
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
                                <Suspense fallback={loading}>
                                    <AppRoutes />
                                    <PlayerRoutes />
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
