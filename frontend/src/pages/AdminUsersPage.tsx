import { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Typography,
  Box,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  Chip
} from '@mui/material';
import {
  DataGrid,
  GridColDef,
  GridPaginationModel,
  GridSortModel
} from '@mui/x-data-grid';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Search as SearchIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material';
import adminService from '../services/adminService';
import type {
  AdminUserListDto,
  UserFilterDto,
  UpdateUserRoleDto
} from '../types/admin';
import { UserRole, getUserRoleName } from '../types/admin';

const AdminUsersPage = () => {
  const [users, setUsers] = useState<AdminUserListDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Filter state
  const [filter, setFilter] = useState<UserFilterDto>({
    page: 1,
    pageSize: 20,
    sortBy: 'CreatedAt',
    sortDescending: true
  });

  // Dialog states
  const [roleDialogOpen, setRoleDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<AdminUserListDto | null>(null);
  const [selectedRole, setSelectedRole] = useState<UserRole>(UserRole.Customer);

  useEffect(() => {
    fetchUsers();
  }, [filter]);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await adminService.getUsers(filter);
      setUsers(result.items);
      setTotalCount(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load users');
    } finally {
      setLoading(false);
    }
  };

  const handlePaginationChange = (model: GridPaginationModel) => {
    setFilter(prev => ({
      ...prev,
      page: model.page + 1,
      pageSize: model.pageSize
    }));
  };

  const handleSortChange = (model: GridSortModel) => {
    if (model.length > 0) {
      setFilter(prev => ({
        ...prev,
        sortBy: model[0].field,
        sortDescending: model[0].sort === 'desc'
      }));
    }
  };

  const handleSearchChange = (value: string) => {
    setFilter(prev => ({
      ...prev,
      searchTerm: value,
      page: 1
    }));
  };

  const handleRoleFilterChange = (value: string) => {
    setFilter(prev => ({
      ...prev,
      role: value === 'all' ? undefined : parseInt(value) as UserRole,
      page: 1
    }));
  };

  const handleStatusFilterChange = (value: string) => {
    setFilter(prev => ({
      ...prev,
      isActive: value === 'all' ? undefined : value === 'active',
      page: 1
    }));
  };

  const openRoleDialog = (user: AdminUserListDto) => {
    setSelectedUser(user);
    setSelectedRole(user.role);
    setRoleDialogOpen(true);
  };

  const closeRoleDialog = () => {
    setRoleDialogOpen(false);
    setSelectedUser(null);
  };

  const handleUpdateRole = async () => {
    if (!selectedUser) return;

    try {
      const data: UpdateUserRoleDto = { role: selectedRole };
      await adminService.updateUserRole(selectedUser.id, data);
      setSuccess(`Role updated successfully for ${selectedUser.fullName}`);
      closeRoleDialog();
      fetchUsers();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update role');
    }
  };

  const openDeleteDialog = (user: AdminUserListDto) => {
    setSelectedUser(user);
    setDeleteDialogOpen(true);
  };

  const closeDeleteDialog = () => {
    setDeleteDialogOpen(false);
    setSelectedUser(null);
  };

  const handleDeleteUser = async () => {
    if (!selectedUser) return;

    try {
      await adminService.deleteUser(selectedUser.id);
      setSuccess(`User ${selectedUser.fullName} deleted successfully`);
      closeDeleteDialog();
      fetchUsers();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete user');
    }
  };

  const columns: GridColDef[] = [
    {
      field: 'fullName',
      headerName: 'Name',
      width: 200,
      sortable: true
    },
    {
      field: 'email',
      headerName: 'Email',
      width: 250,
      sortable: true
    },
    {
      field: 'role',
      headerName: 'Role',
      width: 150,
      sortable: true,
      renderCell: (params: any) => (
        <Chip
          label={getUserRoleName(params.value as UserRole)}
          color={params.value === UserRole.Admin ? 'error' : 'primary'}
          size="small"
        />
      )
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 120,
      sortable: true,
      renderCell: (params: any) => (
        <Chip
          label={params.value ? 'Active' : 'Inactive'}
          color={params.value ? 'success' : 'default'}
          size="small"
        />
      )
    },
    {
      field: 'emailConfirmed',
      headerName: 'Email Verified',
      width: 130,
      sortable: true,
      renderCell: (params: any) => (
        <Chip
          label={params.value ? 'Yes' : 'No'}
          color={params.value ? 'success' : 'warning'}
          size="small"
          variant="outlined"
        />
      )
    },
    {
      field: 'createdAt',
      headerName: 'Created',
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
      width: 120,
      sortable: false,
      renderCell: (params: any) => (
        <Box>
          <IconButton
            size="small"
            color="primary"
            onClick={() => openRoleDialog(params.row)}
            title="Edit Role"
          >
            <EditIcon fontSize="small" />
          </IconButton>
          <IconButton
            size="small"
            color="error"
            onClick={() => openDeleteDialog(params.row)}
            title="Delete User"
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Box>
      )
    }
  ];

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" gutterBottom>
          User Management
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Manage system users, roles, and permissions
        </Typography>
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

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
          <TextField
            placeholder="Search by name, email, or phone..."
            variant="outlined"
            size="small"
            sx={{ flex: '1 1 300px' }}
            onChange={(e) => handleSearchChange(e.target.value)}
            InputProps={{
              startAdornment: <SearchIcon sx={{ mr: 1, color: 'text.secondary' }} />
            }}
          />

          <FormControl size="small" sx={{ minWidth: 150 }}>
            <InputLabel>Role</InputLabel>
            <Select
              label="Role"
              defaultValue="all"
              onChange={(e) => handleRoleFilterChange(e.target.value)}
            >
              <MenuItem value="all">All Roles</MenuItem>
              <MenuItem value={UserRole.Customer.toString()}>Customer</MenuItem>
              <MenuItem value={UserRole.SalonOwner.toString()}>Salon Owner</MenuItem>
              <MenuItem value={UserRole.Staff.toString()}>Staff</MenuItem>
              <MenuItem value={UserRole.Admin.toString()}>Admin</MenuItem>
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 150 }}>
            <InputLabel>Status</InputLabel>
            <Select
              label="Status"
              defaultValue="all"
              onChange={(e) => handleStatusFilterChange(e.target.value)}
            >
              <MenuItem value="all">All Status</MenuItem>
              <MenuItem value="active">Active</MenuItem>
              <MenuItem value="inactive">Inactive</MenuItem>
            </Select>
          </FormControl>

          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={fetchUsers}
          >
            Refresh
          </Button>
        </Box>
      </Paper>

      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={users}
          columns={columns}
          rowCount={totalCount}
          loading={loading}
          pageSizeOptions={[10, 20, 50, 100]}
          paginationModel={{
            page: filter.page - 1,
            pageSize: filter.pageSize
          }}
          paginationMode="server"
          sortingMode="server"
          onPaginationModelChange={handlePaginationChange}
          onSortModelChange={handleSortChange}
          disableRowSelectionOnClick
          sx={{
            '& .MuiDataGrid-row:hover': {
              cursor: 'pointer'
            }
          }}
        />
      </Paper>

      {/* Role Edit Dialog */}
      <Dialog open={roleDialogOpen} onClose={closeRoleDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Update User Role</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              User: <strong>{selectedUser?.fullName}</strong>
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Email: <strong>{selectedUser?.email}</strong>
            </Typography>
            <FormControl fullWidth sx={{ mt: 3 }}>
              <InputLabel>Role</InputLabel>
              <Select
                value={selectedRole}
                label="Role"
                onChange={(e) => setSelectedRole(e.target.value as UserRole)}
              >
                <MenuItem value={UserRole.Customer}>Customer</MenuItem>
                <MenuItem value={UserRole.SalonOwner}>Salon Owner</MenuItem>
                <MenuItem value={UserRole.Staff}>Staff</MenuItem>
                <MenuItem value={UserRole.Admin}>Admin</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeRoleDialog}>Cancel</Button>
          <Button onClick={handleUpdateRole} variant="contained" color="primary">
            Update Role
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={closeDeleteDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Delete User</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            This action cannot be undone!
          </Alert>
          <Typography>
            Are you sure you want to delete user <strong>{selectedUser?.fullName}</strong>?
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Email: {selectedUser?.email}
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeDeleteDialog}>Cancel</Button>
          <Button onClick={handleDeleteUser} variant="contained" color="error">
            Delete User
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default AdminUsersPage;
