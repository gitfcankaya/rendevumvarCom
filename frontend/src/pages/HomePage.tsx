import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { contentService } from '../services/contentService';
import type { ContentPage } from '../services/contentService';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  Container,
  Grid,
  Card,
  CardContent,
  Box,
  Chip,
  Stack,
  Avatar,
  Rating,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { 
  ContentCut, 
  Schedule, 
  Person, 
  TrendingUp,
  CheckCircle,
  Phone,
  Email
} from '@mui/icons-material';

const HomePage: React.FC = () => {
  const navigate = useNavigate();
  const [footerLinks, setFooterLinks] = useState<ContentPage[]>([]);

  useEffect(() => {
    const fetchFooterLinks = async () => {
      try {
        const pages = await contentService.getAll();
        // Footer için sadece aktif sayfaları göster
        setFooterLinks(pages.filter(page => page.isActive));
      } catch (error) {
        console.error('Footer linkler yüklenirken hata oluştu:', error);
      }
    };

    fetchFooterLinks();
  }, []);

  const features = [
    {
      icon: <Schedule sx={{ fontSize: 56 }} />,
      title: "7/24 Online Randevu",
      description: "Müşterileriniz istediği zaman randevu alabilir, siz de kolayca yönetebilirsiniz.",
      color: "#667eea"
    },
    {
      icon: <ContentCut sx={{ fontSize: 56 }} />,
      title: "Akıllı Servis Yönetimi", 
      description: "Hizmetlerinizi kategorize edin, dinamik fiyatlandırma ile gelir artırın.",
      color: "#764ba2"
    },
    {
      icon: <Person sx={{ fontSize: 56 }} />,
      title: "Personel Optimizasyonu",
      description: "AI destekli personel çizelgeleme ile verimliliği maksimuma çıkarın.",
      color: "#f093fb"
    },
    {
      icon: <TrendingUp sx={{ fontSize: 56 }} />,
      title: "Analitik Dashboard",
      description: "Gerçek zamanlı raporlar ve öngörüler ile işinizi büyütün.",
      color: "#4facfe"
    }
  ];

  const testimonials = [
    {
      name: "Ayşe Demir",
      salon: "Güzellik Merkezi",
      comment: "RendevumVar sayesinde müşteri memnuniyetimiz %40 arttı!",
      rating: 5,
      avatar: "A"
    },
    {
      name: "Mehmet Yılmaz", 
      salon: "Modern Kuaför",
      comment: "Randevu yönetimi hiç bu kadar kolay olmamıştı.",
      rating: 5,
      avatar: "M"
    },
    {
      name: "Zeynep Kaya",
      salon: "Lüks Spa",
      comment: "Gelirlerimiz 3 ayda %60 arttı. Harika bir sistem!",
      rating: 5,
      avatar: "Z"
    }
  ];

  return (
    <Box sx={{ 
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      position: 'relative',
      overflow: 'hidden'
    }}>
      {/* Animated Background Elements */}
      <Box sx={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        opacity: 0.1,
        backgroundImage: `
          radial-gradient(circle at 20% 20%, rgba(255,255,255,0.3) 0%, transparent 50%),
          radial-gradient(circle at 80% 80%, rgba(255,255,255,0.3) 0%, transparent 50%),
          radial-gradient(circle at 40% 60%, rgba(255,255,255,0.2) 0%, transparent 50%)
        `
      }} />

      {/* Modern Header */}
      <AppBar 
        position="static" 
        sx={{ 
          background: 'rgba(255,255,255,0.1)',
          backdropFilter: 'blur(20px)',
          borderBottom: '1px solid rgba(255,255,255,0.2)',
          boxShadow: 'none'
        }}
      >
        <Toolbar sx={{ py: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', flexGrow: 1 }}>
            <Box sx={{
              width: 40,
              height: 40,
              borderRadius: 2,
              background: 'linear-gradient(45deg, #fff 30%, rgba(255,255,255,0.8) 90%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              mr: 2
            }}>
              <ContentCut sx={{ color: '#667eea', fontSize: 24 }} />
            </Box>
            <Typography 
              variant="h5" 
              component="div" 
              sx={{ 
                fontWeight: 700,
                background: 'linear-gradient(45deg, #fff 30%, rgba(255,255,255,0.8) 90%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                letterSpacing: '-0.02em'
              }}
            >
              RendevumVar
            </Typography>
          </Box>
          <Stack direction="row" spacing={2}>
            <Button 
              variant="outlined" 
              onClick={() => navigate('/login')}
              sx={{ 
                color: 'white',
                borderColor: 'rgba(255,255,255,0.3)',
                backdropFilter: 'blur(10px)',
                '&:hover': {
                  borderColor: 'white',
                  background: 'rgba(255,255,255,0.1)'
                }
              }}
            >
              Giriş Yap
            </Button>
            <Button 
              variant="contained" 
              onClick={() => navigate('/register')}
              sx={{ 
                background: 'linear-gradient(45deg, rgba(255,255,255,0.9) 30%, #fff 90%)',
                color: '#667eea',
                fontWeight: 600,
                '&:hover': {
                  background: 'linear-gradient(45deg, #fff 30%, rgba(255,255,255,0.95) 90%)',
                  transform: 'translateY(-1px)',
                  boxShadow: '0 8px 25px rgba(0,0,0,0.15)'
                },
                transition: 'all 0.3s ease'
              }}
            >
              Ücretsiz Başla
            </Button>
          </Stack>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ position: 'relative', zIndex: 1 }}>
        {/* Hero Section */}
        <Box sx={{ textAlign: 'center', py: { xs: 8, md: 12 } }}>
          <Chip 
            label="🚀 Türkiye'nin #1 Salon Randevu Sistemi" 
            sx={{ 
              mb: 4,
              background: 'rgba(255,255,255,0.2)',
              backdropFilter: 'blur(10px)',
              color: 'white',
              border: '1px solid rgba(255,255,255,0.3)',
              fontSize: '0.9rem',
              py: 2
            }} 
          />
          
          <Typography 
            variant="h1" 
            component="h1" 
            sx={{ 
              fontSize: { xs: '3rem', md: '4.5rem' },
              fontWeight: 800,
              color: 'white',
              mb: 3,
              letterSpacing: '-0.03em',
              lineHeight: 1.1
            }}
          >
            Salonunuzu
            <Box component="span" sx={{ 
              background: 'linear-gradient(45deg, #f093fb 30%, #f5576c 90%)',
              backgroundClip: 'text',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              display: 'block'
            }}>
              Dijitalleştirin
            </Box>
          </Typography>
          
          <Typography 
            variant="h5" 
            sx={{ 
              color: 'rgba(255,255,255,0.9)',
              mb: 6,
              fontWeight: 300,
              lineHeight: 1.6,
              maxWidth: 600,
              mx: 'auto'
            }}
          >
            AI destekli randevu yönetimi ile müşteri deneyimini artırın, 
            gelirlerinizi %60'a kadar yükseltin
          </Typography>

          <Stack 
            direction={{ xs: 'column', sm: 'row' }} 
            spacing={3} 
            justifyContent="center"
            sx={{ mb: 8 }}
          >
            <Button 
              variant="contained" 
              size="large"
              onClick={() => navigate('/register')}
              sx={{ 
                py: 2,
                px: 4,
                fontSize: '1.1rem',
                fontWeight: 600,
                background: 'linear-gradient(45deg, #f093fb 30%, #f5576c 90%)',
                boxShadow: '0 8px 32px rgba(240, 147, 251, 0.4)',
                '&:hover': {
                  transform: 'translateY(-2px)',
                  boxShadow: '0 12px 40px rgba(240, 147, 251, 0.6)'
                },
                transition: 'all 0.3s ease'
              }}
            >
              14 Gün Ücretsiz Dene
            </Button>
            <Button 
              variant="outlined" 
              size="large"
              sx={{ 
                py: 2,
                px: 4,
                fontSize: '1.1rem',
                color: 'white',
                borderColor: 'rgba(255,255,255,0.5)',
                backdropFilter: 'blur(10px)',
                '&:hover': {
                  borderColor: 'white',
                  background: 'rgba(255,255,255,0.1)',
                  transform: 'translateY(-2px)'
                },
                transition: 'all 0.3s ease'
              }}
            >
              Demo İzle
            </Button>
          </Stack>

          {/* Stats */}
          <Grid container spacing={4} justifyContent="center">
            {[
              { number: "10K+", label: "Mutlu Salon" },
              { number: "2M+", label: "Randevu İşlendi" },
              { number: "%98", label: "Müşteri Memnuniyeti" },
              { number: "7/24", label: "Destek" }
            ].map((stat, index) => (
              <Grid key={index} size={{ xs: 6, sm: 3 }}>
                <Box sx={{ 
                  background: 'rgba(255,255,255,0.1)',
                  backdropFilter: 'blur(20px)',
                  borderRadius: 3,
                  p: 3,
                  border: '1px solid rgba(255,255,255,0.2)'
                }}>
                  <Typography variant="h4" sx={{ color: 'white', fontWeight: 700, mb: 1 }}>
                    {stat.number}
                  </Typography>
                  <Typography variant="body2" sx={{ color: 'rgba(255,255,255,0.8)' }}>
                    {stat.label}
                  </Typography>
                </Box>
              </Grid>
            ))}
          </Grid>
        </Box>

        {/* Features Section */}
        <Box sx={{ py: 8 }}>
          <Typography 
            variant="h3" 
            textAlign="center" 
            sx={{ 
              color: 'white', 
              mb: 2,
              fontWeight: 700
            }}
          >
            Neden RendevumVar?
          </Typography>
          <Typography 
            variant="h6" 
            textAlign="center" 
            sx={{ 
              color: 'rgba(255,255,255,0.8)', 
              mb: 8,
              fontWeight: 300
            }}
          >
            Modern salonlar için geliştirilmiş, AI destekli özellikler
          </Typography>

          <Grid container spacing={4}>
            {features.map((feature, index) => (
              <Grid key={index} size={{ xs: 12, md: 6 }}>
                <Card sx={{ 
                  height: '100%',
                  background: 'rgba(255,255,255,0.1)',
                  backdropFilter: 'blur(20px)',
                  border: '1px solid rgba(255,255,255,0.2)',
                  borderRadius: 4,
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    transform: 'translateY(-8px)',
                    boxShadow: '0 20px 60px rgba(0,0,0,0.2)',
                    background: 'rgba(255,255,255,0.15)',
                  }
                }}>
                  <CardContent sx={{ p: 4 }}>
                    <Box sx={{ 
                      width: 80,
                      height: 80,
                      borderRadius: 3,
                      background: `linear-gradient(45deg, ${feature.color} 30%, rgba(255,255,255,0.2) 90%)`,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      mb: 3,
                      color: 'white'
                    }}>
                      {feature.icon}
                    </Box>
                    <Typography variant="h5" sx={{ color: 'white', fontWeight: 600, mb: 2 }}>
                      {feature.title}
                    </Typography>
                    <Typography variant="body1" sx={{ color: 'rgba(255,255,255,0.8)', lineHeight: 1.6 }}>
                      {feature.description}
                    </Typography>
                    <Button 
                      endIcon={<CheckCircle />}
                      sx={{ 
                        mt: 3,
                        color: feature.color,
                        fontWeight: 600
                      }}
                    >
                      Daha Fazla Bilgi
                    </Button>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Box>

        {/* Testimonials */}
        <Box sx={{ py: 8 }}>
          <Typography 
            variant="h3" 
            textAlign="center" 
            sx={{ 
              color: 'white', 
              mb: 8,
              fontWeight: 700
            }}
          >
            Müşterilerimiz Ne Diyor?
          </Typography>

          <Grid container spacing={4}>
            {testimonials.map((testimonial, index) => (
              <Grid key={index} size={{ xs: 12, md: 4 }}>
                <Card sx={{ 
                  height: '100%',
                  background: 'rgba(255,255,255,0.1)',
                  backdropFilter: 'blur(20px)',
                  border: '1px solid rgba(255,255,255,0.2)',
                  borderRadius: 4,
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    transform: 'translateY(-5px)',
                    background: 'rgba(255,255,255,0.15)',
                  }
                }}>
                  <CardContent sx={{ p: 4, textAlign: 'center' }}>
                    <Avatar sx={{ 
                      width: 60, 
                      height: 60, 
                      mx: 'auto', 
                      mb: 3,
                      background: 'linear-gradient(45deg, #f093fb 30%, #f5576c 90%)',
                      fontSize: '1.5rem',
                      fontWeight: 600
                    }}>
                      {testimonial.avatar}
                    </Avatar>
                    <Rating value={testimonial.rating} readOnly sx={{ mb: 2 }} />
                    <Typography variant="body1" sx={{ 
                      color: 'rgba(255,255,255,0.9)', 
                      mb: 3,
                      fontStyle: 'italic',
                      lineHeight: 1.6
                    }}>
                      "{testimonial.comment}"
                    </Typography>
                    <Typography variant="h6" sx={{ color: 'white', fontWeight: 600 }}>
                      {testimonial.name}
                    </Typography>
                    <Typography variant="body2" sx={{ color: 'rgba(255,255,255,0.7)' }}>
                      {testimonial.salon}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Box>

        {/* CTA Section */}
        <Box sx={{ 
          py: 10,
          textAlign: 'center',
          background: 'rgba(255,255,255,0.1)',
          backdropFilter: 'blur(20px)',
          borderRadius: 6,
          border: '1px solid rgba(255,255,255,0.2)',
          mb: 8
        }}>
          <Typography variant="h3" sx={{ color: 'white', fontWeight: 700, mb: 3 }}>
            Bugün Başlayın, Yarın Farkı Görün
          </Typography>
          <Typography variant="h6" sx={{ 
            color: 'rgba(255,255,255,0.8)', 
            mb: 6,
            maxWidth: 600,
            mx: 'auto',
            lineHeight: 1.6
          }}>
            Kurulum ücretsiz, kredi kartı gerektirmiyor. 14 gün boyunca tüm özelliklerini deneyin.
          </Typography>
          
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} justifyContent="center" flexWrap="wrap">
            <Button 
              variant="contained" 
              size="large"
              onClick={() => navigate('/register')}
              sx={{ 
                py: 2.5,
                px: 6,
                fontSize: '1.2rem',
                fontWeight: 600,
                background: 'linear-gradient(45deg, #f093fb 30%, #f5576c 90%)',
                boxShadow: '0 8px 32px rgba(240, 147, 251, 0.4)',
                '&:hover': {
                  transform: 'translateY(-2px)',
                  boxShadow: '0 12px 40px rgba(240, 147, 251, 0.6)'
                },
                transition: 'all 0.3s ease'
              }}
            >
              Hemen Başla
            </Button>

            <Button 
              variant="contained" 
              size="large"
              onClick={() => navigate('/appointments')}
              sx={{ 
                py: 2.5,
                px: 6,
                fontSize: '1.1rem',
                fontWeight: 600,
                background: 'linear-gradient(45deg, #667eea 30%, #764ba2 90%)',
                color: 'white',
                boxShadow: '0 8px 32px rgba(102, 126, 234, 0.4)',
                '&:hover': {
                  transform: 'translateY(-2px)',
                  boxShadow: '0 12px 40px rgba(102, 126, 234, 0.6)'
                },
                transition: 'all 0.3s ease'
              }}
            >
              📅 Randevular
            </Button>

            <Button 
              variant="contained" 
              size="large"
              onClick={() => navigate('/services')}
              sx={{ 
                py: 2.5,
                px: 6,
                fontSize: '1.1rem',
                fontWeight: 600,
                background: 'linear-gradient(45deg, #11998e 30%, #38ef7d 90%)',
                color: 'white',
                boxShadow: '0 8px 32px rgba(17, 153, 142, 0.4)',
                '&:hover': {
                  transform: 'translateY(-2px)',
                  boxShadow: '0 12px 40px rgba(17, 153, 142, 0.6)'
                },
                transition: 'all 0.3s ease'
              }}
            >
              🔧 Hizmetler
            </Button>
            
            <Button 
              variant="outlined" 
              size="large"
              startIcon={<Phone />}
              sx={{ 
                py: 2.5,
                px: 6,
                fontSize: '1.1rem',
                color: 'white',
                borderColor: 'rgba(255,255,255,0.5)',
                '&:hover': {
                  borderColor: 'white',
                  background: 'rgba(255,255,255,0.1)'
                }
              }}
            >
              Demo Talep Et
            </Button>
          </Stack>
        </Box>
      </Container>

      {/* Footer */}
      <Box sx={{ 
        background: 'rgba(0,0,0,0.2)',
        backdropFilter: 'blur(20px)',
        borderTop: '1px solid rgba(255,255,255,0.1)',
        py: 6
      }}>
        <Container maxWidth="lg">
          <Grid container spacing={4}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <Box sx={{
                  width: 40,
                  height: 40,
                  borderRadius: 2,
                  background: 'linear-gradient(45deg, #f093fb 30%, #f5576c 90%)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mr: 2
                }}>
                  <ContentCut sx={{ color: 'white', fontSize: 24 }} />
                </Box>
                <Typography variant="h5" sx={{ color: 'white', fontWeight: 700 }}>
                  RendevumVar
                </Typography>
              </Box>
              <Typography variant="body1" sx={{ color: 'rgba(255,255,255,0.8)', mb: 3 }}>
                Türkiye'nin en gelişmiş salon randevu yönetim sistemi. 
                Modern teknoloji ile salonunuzu geleceğe taşıyın.
              </Typography>
              <Stack direction="row" spacing={2}>
                <Button startIcon={<Email />} sx={{ color: 'rgba(255,255,255,0.8)' }}>
                  destek@rendevumvar.com
                </Button>
              </Stack>
            </Grid>
            
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="h6" sx={{ color: 'white', mb: 3, fontWeight: 600 }}>
                Hızlı Linkler
              </Typography>
              <Stack spacing={1}>
                {footerLinks.map((page) => (
                  <Button 
                    key={page.id}
                    component={Link}
                    to={`/sayfa/${page.slug}`}
                    sx={{ 
                      color: 'rgba(255,255,255,0.8)',
                      justifyContent: 'flex-start',
                      '&:hover': { color: 'white' }
                    }}
                  >
                    {page.title}
                  </Button>
                ))}
                {/* Statik linkler */}
                <Button 
                  sx={{ 
                    color: 'rgba(255,255,255,0.8)',
                    justifyContent: 'flex-start',
                    '&:hover': { color: 'white' }
                  }}
                >
                  Fiyatlandırma
                </Button>
                <Button 
                  sx={{ 
                    color: 'rgba(255,255,255,0.8)',
                    justifyContent: 'flex-start',
                    '&:hover': { color: 'white' }
                  }}
                >
                  Destek
                </Button>
              </Stack>
            </Grid>
          </Grid>
          
          <Box sx={{ 
            textAlign: 'center', 
            pt: 4, 
            mt: 4, 
            borderTop: '1px solid rgba(255,255,255,0.1)' 
          }}>
            <Typography variant="body2" sx={{ color: 'rgba(255,255,255,0.6)' }}>
              © 2025 RendevumVar. Tüm hakları saklıdır.
            </Typography>
          </Box>
        </Container>
      </Box>
    </Box>
  );
};

export default HomePage;