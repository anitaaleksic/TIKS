import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import axios from 'axios';

export default function EntityList({
  addRoute,
  fetchUrl,
  backgroundImage,
  renderFields,
  onEdit,
  onInfo,
  onDelete,
  idField = 'id'  // podrazumevano 'id'
}) {
  const navigate = useNavigate();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  const handleAdd = () => {
    navigate(addRoute);
  };

  const fetchItems = async () => {
    try {
      const res = await axios.get(fetchUrl);
      setItems(res.data);
      setErrorMsg('');
    } catch (err) {
      console.error(err);
      setErrorMsg(err.response?.data || 'Error fetching data.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchItems();
  }, []);

  return (
    <div className="entity-page-wrapper">
      <div className="entity-header">
        <button onClick={handleAdd} className="form-button large">
          Add
        </button>
      </div>

      {loading ? (
        <p>Loading...</p>
      ) : errorMsg ? (
        <p style={{ color: 'red' }}>{errorMsg}</p>
      ) : (
        <div className="entity-grid">
          {items.map(item => {
            const id = item[idField];  // dinamički uzima id iz odgovarajućeg polja
            return (
              <div key={id} className="entity-card">
                <div
                  className="entity-background-image"
                  style={{ backgroundImage: `url(${backgroundImage})` }}
                ></div>
                {renderFields(item)}
                <div className="entity-buttons">
                  <button onClick={() => onEdit(id)} className="form-button small">Edit</button>
                  <button onClick={() => onInfo(id)} className="form-button small">Info</button>
                  <button onClick={() => onDelete(id)} className="form-button small delete">Delete</button>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

