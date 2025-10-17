import { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  Chip,
  TextField,
  Card,
  CardContent,
  Grid,
  Divider
} from '@mui/material';
import {
  DataGrid,
  GridColDef
} from '@mui/x-data-grid';
import {
  CheckCircle as ApproveIcon,
  Cancel as RejectIcon,
  Info as InfoIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material';
import adminService from '../services/adminService';
import type {
  PendingSalonDto,
  ApproveSalonDto,
  RejectSalonDto
} from '../types/admin';
import { SalonStatus, getSalonStatusName, getSalonStatusColor } from '../types/admin';

const SalonApprovalsPage = () => {
  const [salons, setSalons] = useState<PendingSalonDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Dialog states
  const [approveDialogOpen, setApproveDialogOpen] = useState(false);
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [selectedSalon, setSelectedSalon] = useState<PendingSalonDto | null>(null);
  const [approvalNotes, setApprovalNotes] = useState('');
  const [rejectionReason, setRejectionReason] = useState('');

  useEffect(() => {
    fetchPendingSalons();
  }, []);

  const fetchPendingSalons = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await adminService.getPendingSalons();
      setSalons(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load pending salons');
    } finally {
      setLoading(false);
    }
  };

  const openApproveDialog = (salon: PendingSalonDto) => {
    setSelectedSalon(salon);
    setApprovalNotes('');
    setApproveDialogOpen(true);
  };

  const closeApproveDialog = () => {
    setApproveDialogOpen(false);
    setSelectedSalon(null);
    setApprovalNotes('');
  };

  const handleApproveSalon = async () => {
    if (!selectedSalon) return;

    try {
      const data: ApproveSalonDto = approvalNotes.trim() 
        ? { notes: approvalNotes.trim() }
        : {};
      await adminService.approveSalon(selectedSalon.id, data);
      setSuccess(`Salon "${selectedSalon.name}" approved successfully`);
      closeApproveDialog();
      fetchPendingSalons();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to approve salon');
    }
  };

  const openRejectDialog = (salon: PendingSalonDto) => {
    setSelectedSalon(salon);
    setRejectionReason('');
    setRejectDialogOpen(true);
  };

  const closeRejectDialog = () => {
    setRejectDialogOpen(false);
    setSelectedSalon(null);
    setRejectionReason('');
  };

  const handleRejectSalon = async () => {
    if (!selectedSalon || !rejectionReason.trim()) {
      setError('Rejection reason is required');
      return;
    }

    try {
      const data: RejectSalonDto = {
        rejectionReason: rejectionReason.trim()
      };
      await adminService.rejectSalon(selectedSalon.id, data);
      setSuccess(`Salon "${selectedSalon.name}" rejected successfully`);
      closeRejectDialog();
      fetchPendingSalons();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reject salon');
    }
  };

  const openDetailDialog = (salon: PendingSalonDto) => {
    setSelectedSalon(salon);
    setDetailDialogOpen(true);
  };

  const closeDetailDialog = () => {
    setDetailDialogOpen(false);
    setSelectedSalon(null);
  };

  const columns: GridColDef[] = [
    {
      field: 'name',
      headerName: 'Salon Name',
      width: 200,
      sortable: true
    },
    {
      field: 'ownerName',
      headerName: 'Owner',
      width: 180,
      sortable: true
    },
    {
      field: 'ownerEmail',
      headerName: 'Owner Email',
      width: 220,
      sortable: true
    },
    {
      field: 'city',
      headerName: 'City',
      width: 120,
      sortable: true
    },
    {
      field: 'phone',
      headerName: 'Phone',
      width: 140,
      sortable: true
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      sortable: true,
      renderCell: (params: any) => (
        <Chip
          label={getSalonStatusName(params.value as SalonStatus)}
          color={getSalonStatusColor(params.value as SalonStatus)}
          size="small"
        />
      )
    },
    {
      field: 'createdAt',
      headerName: 'Requested',
      width: 180,
      sortable: true,
      valueFormatter: (value: any) => new Date(value).toLocaleDateString('tr-TR', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 200,
      sortable: false,
      renderCell: (params: any) => (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <IconButton
            size="small"
            color="info"
            onClick={() => openDetailDialog(params.row)}
            title="View Details"
          >
            <InfoIcon fontSize="small" />
          </IconButton>
          <IconButton
            size="small"
            color="success"
            onClick={() => openApproveDialog(params.row)}
            title="Approve"
          >
            <ApproveIcon fontSize="small" />
          </IconButton>
          <IconButton
            size="small"
            color="error"
            onClick={() => openRejectDialog(params.row)}
            title="Reject"
          >
            <RejectIcon fontSize="small" />
          </IconButton>
        </Box>
      )
    }
  ];

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h4" gutterBottom>
            Salon Approvals
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Review and approve or reject salon registration requests
          </Typography>
        </Box>
        <Button
          variant="outlined"
          startIcon={<RefreshIcon />}
          onClick={fetchPendingSalons}
        >
          Refresh
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={salons}
          columns={columns}
          loading={loading}
          pageSizeOptions={[10, 20, 50]}
          disableRowSelectionOnClick
          initialState={{
            pagination: { paginationModel: { pageSize: 20 } },
          }}
          sx={{
            '& .MuiDataGrid-row:hover': {
              cursor: 'pointer'
            }
          }}
        />
      </Paper>

      {/* Detail Dialog */}
      <Dialog open={detailDialogOpen} onClose={closeDetailDialog} maxWidth="md" fullWidth>
        <DialogTitle>Salon Details</DialogTitle>
        <DialogContent>
          {selectedSalon && (
            <Box sx={{ pt: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Salon Information
                      </Typography>
                      <Divider sx={{ mb: 2 }} />
                      <Grid container spacing={2}>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Name
                          </Typography>
                          <Typography variant="body1" fontWeight={500}>
                            {selectedSalon.name}
                          </Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Phone
                          </Typography>
                          <Typography variant="body1" fontWeight={500}>
                            {selectedSalon.phone}
                          </Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Email
                          </Typography>
                          <Typography variant="body1" fontWeight={500}>
                            {selectedSalon.email}
                          </Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            City
                          </Typography>
                          <Typography variant="body1" fontWeight={500}>
                            {selectedSalon.city}
                          </Typography>
                        </Grid>
                        <Grid item xs={12}>
                          <Typography variant="body2" color="text.secondary">
                            Address
                          </Typography>
                          <Typography variant="body1" fontWeight={500}>
                            {selectedSalon.address}
                          </Typography>
                        </Grid>
                        {selectedSalon.description && (
                          <Grid item xs={12}>
                            <Typography variant="body2" color="text.secondary">
                              Description
                            </Typography>
                            <Typography variant="body1" fontWeight={500}>
                              {selectedSalon.description}
                            </Typography>
                          </Grid>
                        )}
                      </Grid>
                    </CardContent>
                  </Card>
                </Grid>

                <Grid item xs={12}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Owner Information
                      </Typography>
                      <Divider sx={{ mb: 2 }} />
                      <Grid container spacing={2}>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Name
                          </Typography>
                          <Typography variant="body1" fontWeight={500}>
                            {selectedSalon.ownerName}
                          </Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Email
                          </Typography>
                          <Typography variant="body1" fontWeight={500}>
                            {selectedSalon.ownerEmail}
                          </Typography>
                        </Grid>
                      </Grid>
                    </CardContent>
                  </Card>
                </Grid>

                <Grid item xs={12}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Request Status
                      </Typography>
                      <Divider sx={{ mb: 2 }} />
                      <Grid container spacing={2}>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Status
                          </Typography>
                          <Chip
                            label={getSalonStatusName(selectedSalon.status)}
                            color={getSalonStatusColor(selectedSalon.status)}
                            size="small"
                          />
                        </Grid>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Requested Date
                          </Typography>
                          <Typography variant="body1" fontWeight={500}>
                            {new Date(selectedSalon.createdAt).toLocaleString('tr-TR')}
                          </Typography>
                        </Grid>
                        {selectedSalon.rejectionReason && (
                          <Grid item xs={12}>
                            <Typography variant="body2" color="text.secondary">
                              Rejection Reason
                            </Typography>
                            <Typography variant="body1" fontWeight={500} color="error">
                              {selectedSalon.rejectionReason}
                            </Typography>
                          </Grid>
                        )}
                      </Grid>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={closeDetailDialog}>Close</Button>
          {selectedSalon?.status === SalonStatus.Pending && (
            <>
              <Button
                onClick={() => {
                  closeDetailDialog();
                  openRejectDialog(selectedSalon);
                }}
                variant="outlined"
                color="error"
              >
                Reject
              </Button>
              <Button
                onClick={() => {
                  closeDetailDialog();
                  openApproveDialog(selectedSalon);
                }}
                variant="contained"
                color="success"
              >
                Approve
              </Button>
            </>
          )}
        </DialogActions>
      </Dialog>

      {/* Approve Dialog */}
      <Dialog open={approveDialogOpen} onClose={closeApproveDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Approve Salon</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Alert severity="info" sx={{ mb: 2 }}>
              You are about to approve this salon registration.
            </Alert>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Salon: <strong>{selectedSalon?.name}</strong>
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Owner: <strong>{selectedSalon?.ownerName}</strong>
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={3}
              label="Notes (Optional)"
              value={approvalNotes}
              onChange={(e) => setApprovalNotes(e.target.value)}
              placeholder="Add any notes for this approval..."
              sx={{ mt: 2 }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeApproveDialog}>Cancel</Button>
          <Button onClick={handleApproveSalon} variant="contained" color="success">
            Approve Salon
          </Button>
        </DialogActions>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={rejectDialogOpen} onClose={closeRejectDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Reject Salon</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Alert severity="warning" sx={{ mb: 2 }}>
              Please provide a reason for rejecting this salon registration.
            </Alert>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Salon: <strong>{selectedSalon?.name}</strong>
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Owner: <strong>{selectedSalon?.ownerName}</strong>
            </Typography>
            <TextField
              fullWidth
              required
              multiline
              rows={4}
              label="Rejection Reason"
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              placeholder="Explain why this salon registration is being rejected..."
              error={!rejectionReason.trim() && rejectionReason.length > 0}
              helperText={!rejectionReason.trim() && rejectionReason.length > 0 ? 'Rejection reason is required' : ''}
              sx={{ mt: 2 }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeRejectDialog}>Cancel</Button>
          <Button
            onClick={handleRejectSalon}
            variant="contained"
            color="error"
            disabled={!rejectionReason.trim()}
          >
            Reject Salon
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default SalonApprovalsPage;
