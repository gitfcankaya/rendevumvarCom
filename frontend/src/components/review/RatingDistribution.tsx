import React from 'react';
import {
  Box,
  Typography,
  LinearProgress,
  Paper,
  Stack,
} from '@mui/material';
import StarIcon from '@mui/icons-material/Star';
import type { SalonRating } from '../../types/review';

interface RatingDistributionProps {
  rating: SalonRating;
}

export const RatingDistribution: React.FC<RatingDistributionProps> = ({
  rating,
}) => {
  const getPercentage = (count: number): number => {
    if (rating.totalReviews === 0) return 0;
    return (count / rating.totalReviews) * 100;
  };

  return (
    <Paper elevation={1} sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Rating Distribution
      </Typography>

      <Box display="flex" alignItems="center" gap={2} mb={3}>
        <Typography variant="h3" component="div">
          {rating.averageRating.toFixed(1)}
        </Typography>
        <Box>
          <Box display="flex" alignItems="center">
            {[...Array(5)].map((_, i) => (
              <StarIcon
                key={i}
                sx={{
                  color:
                    i < Math.floor(rating.averageRating)
                      ? 'warning.main'
                      : 'action.disabled',
                }}
              />
            ))}
          </Box>
          <Typography variant="body2" color="text.secondary">
            Based on {rating.totalReviews} reviews
          </Typography>
        </Box>
      </Box>

      <Stack spacing={1}>
        {[5, 4, 3, 2, 1].map((stars) => {
          const count = rating.ratingDistribution[stars] || 0;
          const percentage = getPercentage(count);

          return (
            <Box key={stars} display="flex" alignItems="center" gap={2}>
              <Typography variant="body2" sx={{ minWidth: 60 }}>
                {stars} star{stars !== 1 && 's'}
              </Typography>
              <Box sx={{ flexGrow: 1 }}>
                <LinearProgress
                  variant="determinate"
                  value={percentage}
                  sx={{
                    height: 8,
                    borderRadius: 1,
                    bgcolor: 'action.hover',
                    '& .MuiLinearProgress-bar': {
                      bgcolor: 'warning.main',
                    },
                  }}
                />
              </Box>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ minWidth: 40, textAlign: 'right' }}
              >
                {count}
              </Typography>
            </Box>
          );
        })}
      </Stack>
    </Paper>
  );
};
