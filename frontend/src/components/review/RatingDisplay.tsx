import React from 'react';
import { Rating as MuiRating, Box, Typography } from '@mui/material';
import StarIcon from '@mui/icons-material/Star';

interface RatingDisplayProps {
  value: number;
  totalReviews?: number;
  showCount?: boolean;
  size?: 'small' | 'medium' | 'large';
}

export const RatingDisplay: React.FC<RatingDisplayProps> = ({
  value,
  totalReviews,
  showCount = true,
  size = 'medium',
}) => {
  return (
    <Box display="flex" alignItems="center" gap={1}>
      <MuiRating
        value={value}
        readOnly
        precision={0.1}
        size={size}
        emptyIcon={<StarIcon style={{ opacity: 0.3 }} fontSize="inherit" />}
      />
      <Typography variant="body2" color="text.secondary">
        {value.toFixed(1)}
        {showCount && totalReviews !== undefined && (
          <span> ({totalReviews})</span>
        )}
      </Typography>
    </Box>
  );
};
