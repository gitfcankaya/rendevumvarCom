import React, { useState } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Avatar,
  Rating,
  Chip,
  IconButton,
  Menu,
  MenuItem,
  Divider,
  TextField,
  Button,
  Alert,
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import ReplyIcon from '@mui/icons-material/Reply';
import { formatDistanceToNow } from 'date-fns';
import type { Review } from '../../types/review';

interface ReviewCardProps {
  review: Review;
  canEdit?: boolean;
  canRespond?: boolean;
  canTogglePublish?: boolean;
  onEdit?: (review: Review) => void;
  onDelete?: (reviewId: string) => void;
  onRespond?: (reviewId: string, response: string) => Promise<void>;
  onTogglePublish?: (reviewId: string) => Promise<void>;
}

export const ReviewCard: React.FC<ReviewCardProps> = ({
  review,
  canEdit = false,
  canRespond = false,
  canTogglePublish = false,
  onEdit,
  onDelete,
  onRespond,
  onTogglePublish,
}) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [showResponseForm, setShowResponseForm] = useState(false);
  const [responseText, setResponseText] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleEdit = () => {
    handleMenuClose();
    if (onEdit) {
      onEdit(review);
    }
  };

  const handleDelete = () => {
    handleMenuClose();
    if (onDelete && confirm('Are you sure you want to delete this review?')) {
      onDelete(review.id);
    }
  };

  const handleTogglePublish = async () => {
    handleMenuClose();
    if (onTogglePublish) {
      try {
        await onTogglePublish(review.id);
      } catch (err) {
        setError('Failed to update publish status');
      }
    }
  };

  const handleSubmitResponse = async () => {
    if (!responseText.trim() || !onRespond) return;

    setLoading(true);
    setError(null);

    try {
      await onRespond(review.id, responseText.trim());
      setShowResponseForm(false);
      setResponseText('');
    } catch (err) {
      setError('Failed to submit response');
    } finally {
      setLoading(false);
    }
  };

  const showMenu = canEdit || canTogglePublish;

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent>
        <Box display="flex" justifyContent="space-between" alignItems="flex-start">
          <Box display="flex" gap={2} flex={1}>
            <Avatar sx={{ bgcolor: 'primary.main' }}>
              {review.customerName.charAt(0).toUpperCase()}
            </Avatar>
            <Box flex={1}>
              <Box display="flex" alignItems="center" gap={1} flexWrap="wrap">
                <Typography variant="subtitle1" fontWeight="bold">
                  {review.customerName}
                </Typography>
                {!review.isPublished && (
                  <Chip label="Unpublished" size="small" color="warning" />
                )}
                <Typography variant="caption" color="text.secondary">
                  {formatDistanceToNow(new Date(review.createdAt), {
                    addSuffix: true,
                  })}
                </Typography>
              </Box>
              <Rating value={review.rating} readOnly size="small" sx={{ my: 0.5 }} />
              {review.staffName && (
                <Typography variant="caption" color="text.secondary" display="block">
                  Service by: {review.staffName}
                </Typography>
              )}
              {review.comment && (
                <Typography variant="body2" sx={{ mt: 1 }}>
                  {review.comment}
                </Typography>
              )}

              {/* Salon Response */}
              {review.response && (
                <Box
                  sx={{
                    mt: 2,
                    pl: 2,
                    borderLeft: 3,
                    borderColor: 'primary.main',
                  }}
                >
                  <Box display="flex" alignItems="center" gap={1} mb={0.5}>
                    <ReplyIcon fontSize="small" color="primary" />
                    <Typography variant="subtitle2" fontWeight="bold">
                      Response from {review.salonName}
                    </Typography>
                  </Box>
                  <Typography variant="body2" color="text.secondary">
                    {review.response}
                  </Typography>
                  {review.responseAt && (
                    <Typography variant="caption" color="text.secondary">
                      {formatDistanceToNow(new Date(review.responseAt), {
                        addSuffix: true,
                      })}
                    </Typography>
                  )}
                </Box>
              )}

              {/* Response Form */}
              {showResponseForm && (
                <Box sx={{ mt: 2 }}>
                  <TextField
                    fullWidth
                    multiline
                    rows={3}
                    placeholder="Write your response..."
                    value={responseText}
                    onChange={(e) => setResponseText(e.target.value)}
                    disabled={loading}
                  />
                  {error && (
                    <Alert severity="error" sx={{ mt: 1 }}>
                      {error}
                    </Alert>
                  )}
                  <Box display="flex" gap={1} mt={1}>
                    <Button
                      size="small"
                      onClick={() => setShowResponseForm(false)}
                      disabled={loading}
                    >
                      Cancel
                    </Button>
                    <Button
                      size="small"
                      variant="contained"
                      onClick={handleSubmitResponse}
                      disabled={loading || !responseText.trim()}
                    >
                      {loading ? 'Submitting...' : 'Submit Response'}
                    </Button>
                  </Box>
                </Box>
              )}

              {/* Respond Button */}
              {canRespond && !review.response && !showResponseForm && (
                <Button
                  size="small"
                  startIcon={<ReplyIcon />}
                  onClick={() => setShowResponseForm(true)}
                  sx={{ mt: 1 }}
                >
                  Respond
                </Button>
              )}
            </Box>
          </Box>

          {showMenu && (
            <>
              <IconButton size="small" onClick={handleMenuOpen}>
                <MoreVertIcon />
              </IconButton>
              <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleMenuClose}
              >
                {canEdit && [
                  <MenuItem key="edit" onClick={handleEdit}>
                    Edit Review
                  </MenuItem>,
                  <MenuItem key="delete" onClick={handleDelete}>
                    Delete Review
                  </MenuItem>,
                  <Divider key="divider" />,
                ]}
                {canTogglePublish && (
                  <MenuItem onClick={handleTogglePublish}>
                    {review.isPublished ? 'Unpublish' : 'Publish'}
                  </MenuItem>
                )}
              </Menu>
            </>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};
