import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
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
import { useForm } from 'react-hook-form';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { resetPassword } from '../store/slices/authSlice';

interface ResetPasswordFormData {
  newPassword: string;
  confirmPassword: string;
}

export const ResetPasswordPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';
  const dispatch = useAppDispatch();
  const { isLoading, error } = useAppSelector((state) => state.auth);
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<ResetPasswordFormData>();

  const password = watch('newPassword');

  const onSubmit = async (data: ResetPasswordFormData) => {
    try {
      await dispatch(resetPassword({ token, newPassword: data.newPassword })).unwrap();
      setSuccess(true);
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err) {
      // Error handled by Redux
    }
  };

  if (!token) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          display: 'flex',
          alignItems: 'center',
        }}
      >
        <Container maxWidth="sm">
          <Paper elevation={24} sx={{ p: 6, borderRadius: 4 }}>
            <Alert severity="error">
              Geçersiz veya eksik sıfırlama bağlantısı. Lütfen e-postanızı kontrol edin.
            </Alert>
          </Paper>
        </Container>
      </Box>
    );
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        display: 'flex',
        alignItems: 'center',
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
          <Typography component="h1" variant="h4" align="center" gutterBottom>
            Yeni Şifre Belirle
          </Typography>

          <Typography variant="body1" align="center" color="textSecondary" sx={{ mb: 4 }}>
            Lütfen yeni şifrenizi belirleyin.
          </Typography>

          {success ? (
            <Alert severity="success" sx={{ mb: 2 }}>
              Şifreniz başarıyla değiştirildi. Giriş sayfasına yönlendiriliyorsunuz...
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
                  label="Yeni Şifre"
                  type="password"
                  id="newPassword"
                  autoFocus
                  {...register('newPassword', {
                    required: 'Yeni şifre gereklidir',
                    minLength: {
                      value: 8,
                      message: 'Şifre en az 8 karakter olmalıdır',
                    },
                    pattern: {
                      value: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
                      message: 'Şifre en az bir büyük harf, bir küçük harf ve bir rakam içermelidir',
                    },
                  })}
                  error={!!errors.newPassword}
                  helperText={errors.newPassword?.message}
                />

                <TextField
                  margin="normal"
                  required
                  fullWidth
                  label="Şifre Tekrar"
                  type="password"
                  id="confirmPassword"
                  {...register('confirmPassword', {
                    required: 'Şifre tekrarı gereklidir',
                    validate: (value) => value === password || 'Şifreler eşleşmiyor',
                  })}
                  error={!!errors.confirmPassword}
                  helperText={errors.confirmPassword?.message}
                />

                <Button
                  type="submit"
                  fullWidth
                  variant="contained"
                  sx={{ mt: 3, mb: 2 }}
                  disabled={isLoading}
                >
                  {isLoading ? <CircularProgress size={24} /> : 'Şifreyi Değiştir'}
                </Button>
              </Box>
            </>
          )}
        </Paper>
      </Container>
    </Box>
  );
};

export default ResetPasswordPage;
