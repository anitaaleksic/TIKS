import { useParams, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import axios from 'axios';

export default function ExtraServiceInfo() {
  const { serviceName } = useParams();
  const navigate = useNavigate();
  const [service, setService] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    axios.get(`/api/ExtraService/GetExtraServiceByName/${serviceName}`)
      .then(res => setService(res.data))
      .catch(err => {
        console.error(err);
        setError('Failed to load extra service details.');
      });
  }, [serviceName]);

  if (error) {
    return (
      <div className="extraservice-form" style={{ maxWidth: '400px', margin: '100px auto' }}>
        <p style={{ color: 'red' }}>{error}</p>
        <button className="form-button" onClick={() => navigate(-1)}>Back</button>
      </div>
    );
  }

  if (!service) {
    return <p style={{ textAlign: 'center', marginTop: '2rem' }}>Loading...</p>;
  }

  return (
    <div className="extraservice-form">
      <h2>Extra Service Info</h2>

      <div className="form-group">
        <label className="form-label">Name:</label>
        <p>{service.serviceName}</p>
      </div>

      <div className="form-group">
        <label className="form-label">Price:</label>
        <p>${service.price.toFixed(2)}</p>
      </div>

      <div className="form-group">
        <label className="form-label">Description:</label>
        <p>{service.description}</p>
      </div>

      <button className="form-button" onClick={() => navigate(-1)}>Back</button>
    </div>
  );
}
