import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Rating,
  TextField,
  Box,
  Typography,
  Alert,
} from '@mui/material';
import StarIcon from '@mui/icons-material/Star';
import type { CreateReviewDto } from '../../types/review';

interface ReviewFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (dto: CreateReviewDto) => Promise<void>;
  appointmentId: string;
  salonId: string;
  staffId?: string;
  salonName: string;
  staffName?: string;
}

export const ReviewFormDialog: React.FC<ReviewFormDialogProps> = ({
  open,
  onClose,
  onSubmit,
  appointmentId,
  salonId,
  staffId,
  salonName,
  staffName,
}) => {
  const [rating, setRating] = useState<number>(5);
  const [comment, setComment] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (rating < 1) {
      setError('Please select a rating');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await onSubmit({
        appointmentId,
        salonId,
        staffId,
        rating,
        comment: comment.trim() || undefined,
      });
      onClose();
      // Reset form
      setRating(5);
      setComment('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to submit review');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      onClose();
      setError(null);
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>Write a Review</DialogTitle>
      <DialogContent>
        <Box sx={{ pt: 1 }}>
          <Typography variant="body1" gutterBottom>
            How was your experience at <strong>{salonName}</strong>
            {staffName && (
              <>
                {' '}
                with <strong>{staffName}</strong>
              </>
            )}
            ?
          </Typography>

          <Box sx={{ my: 3 }}>
            <Typography component="legend" gutterBottom>
              Your Rating *
            </Typography>
            <Rating
              name="rating"
              value={rating}
              onChange={(_, newValue) => {
                if (newValue !== null) {
                  setRating(newValue);
                }
              }}
              size="large"
              emptyIcon={<StarIcon style={{ opacity: 0.3 }} fontSize="inherit" />}
            />
          </Box>

          <TextField
            label="Your Review (Optional)"
            multiline
            rows={4}
            fullWidth
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder="Share your experience..."
            helperText={`${comment.length}/500 characters`}
            inputProps={{ maxLength: 500 }}
          />

          {error && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {error}
            </Alert>
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={loading}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading || rating < 1}
        >
          {loading ? 'Submitting...' : 'Submit Review'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
