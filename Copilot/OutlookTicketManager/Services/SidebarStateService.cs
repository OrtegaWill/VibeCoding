namespace OutlookTicketManager.Services
{
    public class SidebarStateService
    {
        private bool _isCollapsed = false;

        public bool IsCollapsed => _isCollapsed;

        public event Action? OnChange;

        public void SetCollapsed(bool collapsed)
        {
            if (_isCollapsed != collapsed)
            {
                _isCollapsed = collapsed;
                NotifyStateChanged();
            }
        }

        public void Toggle()
        {
            _isCollapsed = !_isCollapsed;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            try
            {
                OnChange?.Invoke();
            }
            catch (Exception ex)
            {
                // Log error si hay sistema de logging disponible
                Console.WriteLine($"Error in SidebarStateService.NotifyStateChanged: {ex.Message}");
            }
        }
    }
}
