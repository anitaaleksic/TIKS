// src/pages/EditExtraService.jsx
import { useParams, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import axios from 'axios';

export default function EditExtraService() {
  const { serviceName } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    serviceName: '',
    price: '',
    description: ''
  });

  const [originalName, setOriginalName] = useState('');
  const [errorMessages, setErrorMessages] = useState([]);

  useEffect(() => {
    axios.get(`/api/ExtraService/GetExtraServiceByName/${serviceName}`)
      .then(res => {
        setFormData(res.data);
        setOriginalName(res.data.serviceName);
      })
      .catch(err => {
        console.error(err);
        alert('Failed to load extra service data.');
        navigate('/extraservice');
      });
  }, [serviceName, navigate]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const errors = [];
    if (!formData.serviceName) errors.push("Service name is required.");
    if (!formData.price || isNaN(formData.price)) errors.push("Valid price is required.");

    if (errors.length > 0) {
      setErrorMessages(errors);
      return;
    }

    axios.put(`/api/ExtraService/UpdateExtraServiceByName/${originalName}`, formData)
      .then(() => {
        alert('Extra service updated successfully!');
        navigate('/extraservice');
      })
      .catch(err => {
        console.error(err);
        setErrorMessages([err.response?.data?.message || "Update failed."]);
      });
  };

  return (
    <form className="extraservice-form" onSubmit={handleSubmit}>
      <h2>Edit Extra Service</h2>

      <div className="form-group">
        <label className="form-label">Service Name:</label>
        <input
          className="form-input"
          name="serviceName"
          type="text"
          value={formData.serviceName}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label">Price:</label>
        <input
          className="form-input"
          name="price"
          type="number"
          step="0.01"
          value={formData.price}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label">Description:</label>
        <textarea
          className="textarea-input"
          name="description"
          value={formData.description}
          onChange={handleChange}
        />
      </div>

      <button type="submit" className="form-button">Update Service</button>

      {errorMessages.length > 0 && (
        <div style={{ color: 'red', marginTop: '1rem' }}>
          <ul>
            {errorMessages.map((msg, idx) => <li key={idx}>{msg}</li>)}
          </ul>
        </div>
      )}
    </form>
  );
}
