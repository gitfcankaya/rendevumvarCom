import React, { useState } from 'react';
import {
  Container,
  Paper,
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  Link,
  IconButton,
  InputAdornment,
  Divider,
  Checkbox,
  FormControlLabel,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { register } from '../store/slices/authSlice';
import type { AppDispatch, RootState } from '../store/store';
import { 
  Visibility, 
  VisibilityOff, 
  ContentCut,
  Google,
  Apple,
  ArrowBack,
  Person,
  Email,
  Business
} from '@mui/icons-material';

const RegisterPage: React.FC = () => {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [tenantName, setTenantName] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [acceptTerms, setAcceptTerms] = useState(false);
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { isLoading, error } = useSelector((state: RootState) => state.auth);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (password !== confirmPassword) {
      return;
    }

    if (!acceptTerms) {
      return;
    }

    try {
      await dispatch(register({ 
        firstName, 
        lastName, 
        email, 
        password
      })).unwrap();
      navigate('/dashboard');
    } catch (error) {
      // Error is handled in the Redux slice
    }
  };

  return (
    <Box sx={{ 
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      display: 'flex',
      alignItems: 'center',
      position: 'relative',
      overflow: 'hidden',
      py: 4
    }}>
      {/* Background Elements */}
      <Box sx={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        opacity: 0.1,
        backgroundImage: `
          radial-gradient(circle at 20% 20%, rgba(255,255,255,0.3) 0%, transparent 50%),
          radial-gradient(circle at 80% 80%, rgba(255,255,255,0.3) 0%, transparent 50%)
        `
      }} />

      <Container component="main" maxWidth="md" sx={{ position: 'relative', zIndex: 1 }}>
        <Paper 
          elevation={24}
          sx={{ 
            p: 6,
            borderRadius: 4,
            background: 'rgba(255,255,255,0.95)',
            backdropFilter: 'blur(20px)',
            border: '1px solid rgba(255,255,255,0.2)',
            position: 'relative'
          }}
        >
          {/* Back Button */}
          <IconButton 
            onClick={() => navigate('/')}
            sx={{ 
              position: 'absolute',
              top: 16,
              left: 16,
              color: 'text.secondary'
            }}
          >
            <ArrowBack />
          </IconButton>

          {/* Logo and Title */}
          <Box sx={{ textAlign: 'center', mb: 4 }}>
            <Box sx={{
              width: 80,
              height: 80,
              borderRadius: 3,
              background: 'linear-gradient(45deg, #667eea 30%, #764ba2 90%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              mx: 'auto',
              mb: 3
            }}>
              <ContentCut sx={{ color: 'white', fontSize: 40 }} />
            </Box>
            
            <Typography 
              variant="h4" 
              component="h1" 
              sx={{ 
                fontWeight: 700,
                background: 'linear-gradient(45deg, #667eea 30%, #764ba2 90%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                mb: 1
              }}
            >
              Hesap Oluşturun
            </Typography>
            <Typography variant="body1" color="text.secondary">
              RendevumVar ile salonunuzu dijitalleştirin
            </Typography>
          </Box>

          {error && (
            <Alert 
              severity="error" 
              sx={{ 
                mb: 3,
                borderRadius: 2,
                backgroundColor: 'rgba(211, 47, 47, 0.1)',
                border: '1px solid rgba(211, 47, 47, 0.2)'
              }}
            >
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit}>
            {/* Personal Information */}
            <Typography variant="h6" sx={{ mb: 2, color: 'text.primary', fontWeight: 600 }}>
              Kişisel Bilgiler
            </Typography>
            
            <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
              <TextField
                required
                fullWidth
                id="firstName"
                label="Ad"
                name="firstName"
                autoComplete="given-name"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Person sx={{ color: 'text.secondary' }} />
                    </InputAdornment>
                  ),
                }}
                sx={{
                  '& .MuiOutlinedInput-root': {
                    borderRadius: 2,
                    '&:hover fieldset': {
                      borderColor: '#667eea',
                    },
                    '&.Mui-focused fieldset': {
                      borderColor: '#667eea',
                    },
                  },
                  '& .MuiInputLabel-root.Mui-focused': {
                    color: '#667eea',
                  },
                }}
              />
              
              <TextField
                required
                fullWidth
                id="lastName"
                label="Soyad"
                name="lastName"
                autoComplete="family-name"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                sx={{
                  '& .MuiOutlinedInput-root': {
                    borderRadius: 2,
                    '&:hover fieldset': {
                      borderColor: '#667eea',
                    },
                    '&.Mui-focused fieldset': {
                      borderColor: '#667eea',
                    },
                  },
                  '& .MuiInputLabel-root.Mui-focused': {
                    color: '#667eea',
                  },
                }}
              />
            </Box>

            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="E-posta Adresi"
              name="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Email sx={{ color: 'text.secondary' }} />
                  </InputAdornment>
                ),
              }}
              sx={{
                mb: 3,
                '& .MuiOutlinedInput-root': {
                  borderRadius: 2,
                  '&:hover fieldset': {
                    borderColor: '#667eea',
                  },
                  '&.Mui-focused fieldset': {
                    borderColor: '#667eea',
                  },
                },
                '& .MuiInputLabel-root.Mui-focused': {
                  color: '#667eea',
                },
              }}
            />

            {/* Business Information */}
            <Typography variant="h6" sx={{ mb: 2, color: 'text.primary', fontWeight: 600 }}>
              İşletme Bilgileri
            </Typography>

            <TextField
              margin="normal"
              required
              fullWidth
              id="tenantName"
              label="Salon Adı"
              name="tenantName"
              value={tenantName}
              onChange={(e) => setTenantName(e.target.value)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Business sx={{ color: 'text.secondary' }} />
                  </InputAdornment>
                ),
              }}
              sx={{
                mb: 3,
                '& .MuiOutlinedInput-root': {
                  borderRadius: 2,
                  '&:hover fieldset': {
                    borderColor: '#667eea',
                  },
                  '&.Mui-focused fieldset': {
                    borderColor: '#667eea',
                  },
                },
                '& .MuiInputLabel-root.Mui-focused': {
                  color: '#667eea',
                },
              }}
            />

            {/* Security */}
            <Typography variant="h6" sx={{ mb: 2, color: 'text.primary', fontWeight: 600 }}>
              Güvenlik
            </Typography>

            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Şifre"
              type={showPassword ? 'text' : 'password'}
              id="password"
              autoComplete="new-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowPassword(!showPassword)}
                      edge="end"
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              sx={{
                mb: 2,
                '& .MuiOutlinedInput-root': {
                  borderRadius: 2,
                  '&:hover fieldset': {
                    borderColor: '#667eea',
                  },
                  '&.Mui-focused fieldset': {
                    borderColor: '#667eea',
                  },
                },
                '& .MuiInputLabel-root.Mui-focused': {
                  color: '#667eea',
                },
              }}
            />

            <TextField
              margin="normal"
              required
              fullWidth
              name="confirmPassword"
              label="Şifre Tekrar"
              type={showConfirmPassword ? 'text' : 'password'}
              id="confirmPassword"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              error={confirmPassword !== '' && password !== confirmPassword}
              helperText={confirmPassword !== '' && password !== confirmPassword ? 'Şifreler eşleşmiyor' : ''}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                      edge="end"
                    >
                      {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              sx={{
                mb: 3,
                '& .MuiOutlinedInput-root': {
                  borderRadius: 2,
                  '&:hover fieldset': {
                    borderColor: '#667eea',
                  },
                  '&.Mui-focused fieldset': {
                    borderColor: '#667eea',
                  },
                },
                '& .MuiInputLabel-root.Mui-focused': {
                  color: '#667eea',
                },
              }}
            />

            <FormControlLabel
              control={
                <Checkbox
                  checked={acceptTerms}
                  onChange={(e) => setAcceptTerms(e.target.checked)}
                  sx={{
                    color: '#667eea',
                    '&.Mui-checked': {
                      color: '#667eea',
                    },
                  }}
                />
              }
              label={
                <Typography variant="body2" color="text.secondary">
                  <Link href="#" sx={{ color: '#667eea', textDecoration: 'none' }}>
                    Kullanım şartları
                  </Link>
                  {' '}ve{' '}
                  <Link href="#" sx={{ color: '#667eea', textDecoration: 'none' }}>
                    gizlilik politikası
                  </Link>
                  nı kabul ediyorum
                </Typography>
              }
              sx={{ mb: 3 }}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              disabled={isLoading || !acceptTerms || password !== confirmPassword}
              sx={{ 
                py: 2,
                mb: 3,
                borderRadius: 2,
                fontSize: '1.1rem',
                fontWeight: 600,
                background: 'linear-gradient(45deg, #667eea 30%, #764ba2 90%)',
                boxShadow: '0 8px 32px rgba(102, 126, 234, 0.4)',
                '&:hover': {
                  background: 'linear-gradient(45deg, #5a6fd8 30%, #6a4190 90%)',
                  transform: 'translateY(-1px)',
                  boxShadow: '0 12px 40px rgba(102, 126, 234, 0.6)',
                },
                '&:disabled': {
                  background: 'rgba(0,0,0,0.12)',
                },
                transition: 'all 0.3s ease'
              }}
            >
              {isLoading ? 'Hesap Oluşturuluyor...' : 'Hesap Oluştur'}
            </Button>

            <Box textAlign="center" sx={{ mb: 3 }}>
              <Link
                component="button"
                type="button"
                variant="body2"
                onClick={() => navigate('/login')}
                sx={{ 
                  color: '#667eea',
                  textDecoration: 'none',
                  fontWeight: 500,
                  '&:hover': {
                    textDecoration: 'underline'
                  }
                }}
              >
                Zaten hesabınız var mı? Giriş yapın
              </Link>
            </Box>

            <Divider sx={{ my: 3 }}>
              <Typography variant="body2" color="text.secondary">
                veya
              </Typography>
            </Divider>

            {/* Social Login Buttons */}
            <Box sx={{ display: 'flex', gap: 2 }}>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<Google />}
                sx={{
                  py: 1.5,
                  borderRadius: 2,
                  borderColor: 'rgba(0,0,0,0.12)',
                  color: 'text.secondary',
                  '&:hover': {
                    borderColor: '#db4437',
                    color: '#db4437',
                  }
                }}
              >
                Google ile Kayıt Ol
              </Button>
              <Button
                fullWidth
                variant="outlined"
                startIcon={<Apple />}
                sx={{
                  py: 1.5,
                  borderRadius: 2,
                  borderColor: 'rgba(0,0,0,0.12)',
                  color: 'text.secondary',
                  '&:hover': {
                    borderColor: 'black',
                    color: 'black',
                  }
                }}
              >
                Apple ile Kayıt Ol
              </Button>
            </Box>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default RegisterPage;