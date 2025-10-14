import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Button,
  CircularProgress,
  Alert,
  Paper
} from '@mui/material';
import { ArrowBack } from '@mui/icons-material';
import { contentService } from '../services/contentService';
import type { ContentPage } from '../services/contentService';

const DynamicContentPage: React.FC = () => {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const [content, setContent] = useState<ContentPage | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchContent = async () => {
      if (!slug) {
        setError('Sayfa bulunamadı');
        setLoading(false);
        return;
      }

      try {
        const data = await contentService.getBySlug(slug);
        setContent(data);
      } catch (err) {
        setError('İçerik yüklenirken bir hata oluştu');
        console.error('Content fetch error:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchContent();
  }, [slug]);

  if (loading) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}
      >
        <CircularProgress sx={{ color: 'white' }} />
      </Box>
    );
  }

  if (error || !content) {
    return (
      <Box
        sx={{
          minHeight: '100vh',
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          p: 3
        }}
      >
        <Container maxWidth="md">
          <Alert severity="error" sx={{ mb: 3 }}>
            {error || 'Sayfa bulunamadı'}
          </Alert>
          <Button
            startIcon={<ArrowBack />}
            onClick={() => navigate('/')}
            variant="contained"
            sx={{
              background: 'linear-gradient(45deg, #f093fb 30%, #f5576c 90%)',
              border: 0,
              borderRadius: 3,
              boxShadow: '0 3px 5px 2px rgba(255, 105, 135, .3)',
              color: 'white',
              height: 48,
              padding: '0 30px',
            }}
          >
            Ana Sayfaya Dön
          </Button>
        </Container>
      </Box>
    );
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        py: 8
      }}
    >
      <Container maxWidth="lg">
        <Button
          startIcon={<ArrowBack />}
          onClick={() => navigate('/')}
          sx={{
            color: 'white',
            mb: 4,
            '&:hover': {
              background: 'rgba(255,255,255,0.1)'
            }
          }}
        >
          Ana Sayfaya Dön
        </Button>

        <Paper
          sx={{
            borderRadius: 4,
            overflow: 'hidden',
            background: 'rgba(255,255,255,0.1)',
            backdropFilter: 'blur(20px)',
            border: '1px solid rgba(255,255,255,0.2)',
          }}
        >
          {content.imageUrl && (
            <Box
              sx={{
                height: 300,
                backgroundImage: `url(${content.imageUrl})`,
                backgroundSize: 'cover',
                backgroundPosition: 'center',
                position: 'relative',
                '&::after': {
                  content: '""',
                  position: 'absolute',
                  top: 0,
                  left: 0,
                  right: 0,
                  bottom: 0,
                  background: 'linear-gradient(45deg, rgba(0,0,0,0.3), rgba(0,0,0,0.1))'
                }
              }}
            />
          )}
          
          <Box sx={{ p: 6 }}>
            <Typography
              variant="h2"
              sx={{
                color: 'white',
                fontWeight: 700,
                mb: 4,
                textAlign: 'center'
              }}
            >
              {content.title}
            </Typography>

            <Box
              sx={{
                color: 'rgba(255,255,255,0.9)',
                fontSize: '1.1rem',
                lineHeight: 1.8,
                '& p': { mb: 2 },
                '& h1, & h2, & h3, & h4, & h5, & h6': { 
                  color: 'white',
                  fontWeight: 600,
                  mt: 3,
                  mb: 2
                },
                '& ul, & ol': {
                  pl: 3,
                  mb: 2
                },
                '& li': {
                  mb: 1
                },
                '& a': {
                  color: '#f093fb',
                  textDecoration: 'none',
                  '&:hover': {
                    textDecoration: 'underline'
                  }
                }
              }}
              dangerouslySetInnerHTML={{ __html: content.content }}
            />

            {content.buttonText && content.buttonUrl && (
              <Box sx={{ textAlign: 'center', mt: 6 }}>
                <Button
                  href={content.buttonUrl}
                  variant="contained"
                  sx={{
                    background: 'linear-gradient(45deg, #f093fb 30%, #f5576c 90%)',
                    border: 0,
                    borderRadius: 3,
                    boxShadow: '0 3px 5px 2px rgba(255, 105, 135, .3)',
                    color: 'white',
                    height: 48,
                    padding: '0 30px',
                    fontSize: '1.1rem',
                    fontWeight: 600
                  }}
                >
                  {content.buttonText}
                </Button>
              </Box>
            )}
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default DynamicContentPage;