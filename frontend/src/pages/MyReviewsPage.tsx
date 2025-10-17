import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Rating as MuiRating,
} from '@mui/material';
import StarIcon from '@mui/icons-material/Star';
import { ReviewList } from '../components/review';
import { reviewService } from '../services/reviewService';
import type { Review, UpdateReviewDto } from '../types/review';

export const MyReviewsPage: React.FC = () => {
  const [reviews, setReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingReview, setEditingReview] = useState<Review | null>(null);
  const [editForm, setEditForm] = useState<UpdateReviewDto>({
    rating: 5,
    comment: '',
  });

  useEffect(() => {
    loadReviews();
  }, []);

  const loadReviews = async () => {
    try {
      setLoading(true);
      const data = await reviewService.getMyReviews();
      setReviews(data);
      setError(null);
    } catch (err) {
      setError('Failed to load reviews');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (review: Review) => {
    setEditingReview(review);
    setEditForm({
      rating: review.rating,
      comment: review.comment || '',
    });
  };

  const handleCloseEdit = () => {
    setEditingReview(null);
    setEditForm({ rating: 5, comment: '' });
  };

  const handleSaveEdit = async () => {
    if (!editingReview) return;

    try {
      await reviewService.updateReview(editingReview.id, editForm);
      await loadReviews();
      handleCloseEdit();
    } catch (err) {
      setError('Failed to update review');
      console.error(err);
    }
  };

  const handleDelete = async (reviewId: string) => {
    try {
      await reviewService.deleteReview(reviewId);
      await loadReviews();
    } catch (err) {
      setError('Failed to delete review');
      console.error(err);
    }
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>
        My Reviews
      </Typography>
      <Typography variant="body1" color="text.secondary" paragraph>
        View and manage your reviews
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <ReviewList
        reviews={reviews}
        loading={loading}
        canEdit={true}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      {/* Edit Dialog */}
      <Dialog
        open={editingReview !== null}
        onClose={handleCloseEdit}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Edit Review</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Typography component="legend" gutterBottom>
              Your Rating *
            </Typography>
            <MuiRating
              value={editForm.rating}
              onChange={(_, newValue) => {
                if (newValue !== null) {
                  setEditForm({ ...editForm, rating: newValue });
                }
              }}
              size="large"
              emptyIcon={<StarIcon style={{ opacity: 0.3 }} fontSize="inherit" />}
            />
            <TextField
              label="Your Review (Optional)"
              multiline
              rows={4}
              fullWidth
              value={editForm.comment}
              onChange={(e) =>
                setEditForm({ ...editForm, comment: e.target.value })
              }
              placeholder="Share your experience..."
              helperText={`${editForm.comment?.length || 0}/500 characters`}
              inputProps={{ maxLength: 500 }}
              sx={{ mt: 2 }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseEdit}>Cancel</Button>
          <Button onClick={handleSaveEdit} variant="contained">
            Save Changes
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};
