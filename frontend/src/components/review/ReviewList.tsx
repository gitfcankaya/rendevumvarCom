import React from 'react';
import { Box, Typography, CircularProgress, Alert } from '@mui/material';
import { ReviewCard } from './ReviewCard';
import type { Review } from '../../types/review';

interface ReviewListProps {
  reviews: Review[];
  loading?: boolean;
  error?: string | null;
  canEdit?: boolean;
  canRespond?: boolean;
  canTogglePublish?: boolean;
  onEdit?: (review: Review) => void;
  onDelete?: (reviewId: string) => void;
  onRespond?: (reviewId: string, response: string) => Promise<void>;
  onTogglePublish?: (reviewId: string) => Promise<void>;
}

export const ReviewList: React.FC<ReviewListProps> = ({
  reviews,
  loading = false,
  error = null,
  canEdit = false,
  canRespond = false,
  canTogglePublish = false,
  onEdit,
  onDelete,
  onRespond,
  onTogglePublish,
}) => {
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ my: 2 }}>
        {error}
      </Alert>
    );
  }

  if (reviews.length === 0) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="body1" color="text.secondary">
          No reviews yet. Be the first to review!
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      {reviews.map((review) => (
        <ReviewCard
          key={review.id}
          review={review}
          canEdit={canEdit}
          canRespond={canRespond}
          canTogglePublish={canTogglePublish}
          onEdit={onEdit}
          onDelete={onDelete}
          onRespond={onRespond}
          onTogglePublish={onTogglePublish}
        />
      ))}
    </Box>
  );
};
