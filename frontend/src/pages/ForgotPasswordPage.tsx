import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Container,
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
} from '@mui/material';
import { ArrowBack } from '@mui/icons-material';
import { useForm } from 'react-hook-form';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { forgotPassword } from '../store/slices/authSlice';

interface ForgotPasswordFormData {
  email: string;
}

export const ForgotPasswordPage = () => {
  const dispatch = useAppDispatch();
  const { isLoading, error } = useAppSelector((state) => state.auth);
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormData>();

  const onSubmit = async (data: ForgotPasswordFormData) => {
    try {
      await dispatch(forgotPassword(data.email)).unwrap();
      setSuccess(true);
    } catch (err) {
      // Error handled by Redux
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        display: 'flex',
        alignItems: 'center',
        position: 'relative',
      }}
    >
      <Container maxWidth="sm">
        <Paper
          elevation={24}
          sx={{
            p: 6,
            borderRadius: 4,
            background: 'rgba(255,255,255,0.95)',
          }}
        >
          <Box sx={{ mb: 3 }}>
            <Link to="/login" style={{ textDecoration: 'none' }}>
              <Button startIcon={<ArrowBack />} color="primary">
                Giriş Sayfasına Dön
              </Button>
            </Link>
          </Box>

          <Typography component="h1" variant="h4" align="center" gutterBottom>
            Şifremi Unuttum
          </Typography>

          <Typography variant="body1" align="center" color="textSecondary" sx={{ mb: 4 }}>
            E-posta adresinizi girin, size şifre sıfırlama bağlantısı gönderelim.
          </Typography>

          {success ? (
            <Alert severity="success" sx={{ mb: 2 }}>
              Şifre sıfırlama bağlantısı e-posta adresinize gönderildi. Lütfen gelen kutunuzu
              kontrol edin.
            </Alert>
          ) : (
            <>
              {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {error}
                </Alert>
              )}

              <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
                <TextField
                  margin="normal"
                  required
                  fullWidth
                  id="email"
                  label="E-posta Adresi"
                  autoComplete="email"
                  autoFocus
                  {...register('email', {
                    required: 'E-posta adresi gereklidir',
                    pattern: {
                      value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                      message: 'Geçerli bir e-posta adresi giriniz',
                    },
                  })}
                  error={!!errors.email}
                  helperText={errors.email?.message}
                />

                <Button
                  type="submit"
                  fullWidth
                  variant="contained"
                  sx={{ mt: 3, mb: 2 }}
                  disabled={isLoading}
                >
                  {isLoading ? <CircularProgress size={24} /> : 'Sıfırlama Bağlantısı Gönder'}
                </Button>
              </Box>
            </>
          )}
        </Paper>
      </Container>
    </Box>
  );
};

export default ForgotPasswordPage;
