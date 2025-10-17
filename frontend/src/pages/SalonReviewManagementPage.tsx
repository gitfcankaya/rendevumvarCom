import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Tabs,
  Tab,
  Alert,
  CircularProgress,
} from '@mui/material';
import { reviewService } from '../services/reviewService';
import salonService from '../services/salonService';
import { RatingDisplay, RatingDistribution, ReviewList } from '../components/review';
import type { Review, SalonRating } from '../types/review';
import type { SalonDto } from '../services/salonService';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`review-tabpanel-${index}`}
      aria-labelledby={`review-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

export const SalonReviewManagementPage: React.FC = () => {
  const [salons, setSalons] = useState<SalonDto[]>([]);
  const [selectedSalonId, setSelectedSalonId] = useState<string | null>(null);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [salonRating, setSalonRating] = useState<SalonRating | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tabValue, setTabValue] = useState(0);

  useEffect(() => {
    loadSalons();
  }, []);

  useEffect(() => {
    if (selectedSalonId) {
      loadReviews();
      loadSalonRating();
    }
  }, [selectedSalonId]);

  const loadSalons = async () => {
    try {
      setLoading(true);
      const data = await salonService.getMySalons();
      setSalons(data);
      if (data.length > 0) {
        setSelectedSalonId(data[0].id);
      }
      setError(null);
    } catch (err) {
      setError('Failed to load salons');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadReviews = async () => {
    if (!selectedSalonId) return;
    
    try {
      const data = await reviewService.getReviewsBySalon(selectedSalonId, false);
      setReviews(data);
    } catch (err) {
      console.error('Error loading reviews:', err);
    }
  };

  const loadSalonRating = async () => {
    if (!selectedSalonId) return;

    try {
      const data = await reviewService.getSalonRating(selectedSalonId);
      setSalonRating(data);
    } catch (err) {
      console.error('Error loading salon rating:', err);
    }
  };

  const handleRespond = async (reviewId: string, response: string) => {
    try {
      await reviewService.addResponse(reviewId, { response });
      await loadReviews(); // Reload to show the response
    } catch (err) {
      console.error('Error responding to review:', err);
      throw err;
    }
  };

  const handleTogglePublish = async (reviewId: string) => {
    try {
      await reviewService.togglePublish(reviewId);
      await loadReviews(); // Reload to update publish status
    } catch (err) {
      console.error('Error toggling publish:', err);
      throw err;
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4, textAlign: 'center' }}>
        <CircularProgress />
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error">{error}</Alert>
      </Container>
    );
  }

  if (salons.length === 0) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="info">
          You don't have any salons yet. Create a salon to manage reviews.
        </Alert>
      </Container>
    );
  }

  const selectedSalon = salons.find((s) => s.id === selectedSalonId);

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>
        Review Management
      </Typography>
      <Typography variant="body1" color="text.secondary" paragraph>
        Manage and respond to customer reviews
      </Typography>

      {/* Salon Selector */}
      {salons.length > 1 && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Select Salon
          </Typography>
          <Box display="flex" gap={2} flexWrap="wrap">
            {salons.map((salon) => (
              <Box
                key={salon.id}
                onClick={() => setSelectedSalonId(salon.id)}
                sx={{
                  p: 2,
                  border: 1,
                  borderColor: selectedSalonId === salon.id ? 'primary.main' : 'divider',
                  borderRadius: 1,
                  cursor: 'pointer',
                  bgcolor: selectedSalonId === salon.id ? 'action.selected' : 'background.paper',
                  '&:hover': {
                    borderColor: 'primary.main',
                  },
                }}
              >
                <Typography variant="subtitle1" fontWeight="bold">
                  {salon.name}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {salon.city}
                </Typography>
              </Box>
            ))}
          </Box>
        </Box>
      )}

      {/* Salon Rating Overview */}
      {selectedSalon && salonRating && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="h5" gutterBottom>
            {selectedSalon.name}
          </Typography>
          <RatingDisplay
            value={salonRating.averageRating}
            totalReviews={salonRating.totalReviews}
            size="large"
          />
        </Box>
      )}

      {/* Tabs */}
      <Box sx={{ width: '100%' }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={(_, newValue) => setTabValue(newValue)}>
            <Tab label={`All Reviews (${reviews.length})`} />
            <Tab label="Rating Distribution" />
          </Tabs>
        </Box>

        <TabPanel value={tabValue} index={0}>
          <ReviewList
            reviews={reviews}
            canRespond={true}
            canTogglePublish={true}
            onRespond={handleRespond}
            onTogglePublish={handleTogglePublish}
          />
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          {salonRating && <RatingDistribution rating={salonRating} />}
        </TabPanel>
      </Box>
    </Container>
  );
};
