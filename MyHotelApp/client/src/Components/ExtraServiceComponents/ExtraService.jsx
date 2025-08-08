
import avatarExtraService from '../../assets/ExtraServiceLogo.png'; 
import EntityList from '../EntityList';
import { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

export default function ExtraService() {
  const [refresh, setRefresh] = useState(false);

  const navigate = useNavigate();

  const handleEdit = (extraService) => {
  console.log("Going to info page for service:", extraService);
  navigate(`/extraservice/edit/${extraService}`);
};

 const handleInfo = (extraService) => {
  console.log("Going to info page for service:", extraService);
  navigate(`/extraservice/info/${extraService}`);
};
  const handleDelete = async (serviceName) => {
    if (!window.confirm(`Are you sure you want to delete extra service "${serviceName}"?`)) return;

    try {
      await axios.delete(`/api/ExtraService/DeleteExtraServiceByName/${serviceName}`);
      alert('Extra service deleted successfully.');
      setRefresh(prev => !prev);
    } catch (err) {
      console.error('Error deleting:', err);
      alert('Failed to delete extra service.');
    }
  };

  return (
    <EntityList
      addRoute="/addextraservice"
      fetchUrl="/api/ExtraService/GetAllExtraServices"
      backgroundImage={avatarExtraService}
      renderFields={service => (
        <>
          <p><strong>Name:</strong> {service.serviceName}</p>
          <p><strong>Price:</strong> ${service.price.toFixed(2)}</p>
          <p><strong>Description:</strong> {service.description}</p>
        </>
      )}
      onEdit={handleEdit}
      onInfo={handleInfo}
      onDelete={handleDelete}
      idField="serviceName"
      refreshTrigger={refresh}
    />
  );
}
