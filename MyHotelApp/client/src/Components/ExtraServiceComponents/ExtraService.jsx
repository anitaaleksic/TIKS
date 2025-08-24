import { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

export default function ExtraService() {
  const [extraServices, setExtraServices] = useState([]);

  const navigate = useNavigate();

  async function GetAllExtraServices() {
    const response = await axios.get("/api/ExtraService/GetAllExtraServices");
    return response.data;
  }

  useEffect(() => {
   async function loadExtraServices() {
    const data = await GetAllExtraServices();
    setExtraServices(data);
   } 
   loadExtraServices();
  }, [])

  const handleAdd = () => {
    navigate("/addextraservice");
  }

  const handleEdit = (extraService) => {
    navigate(`/extraservice/edit/${extraService}`);
  };  

  return (
    <div className="entity-page-wrapper">
      <div className="entity-header">
        <button onClick={handleAdd} className="form-button large">
          Add Extra Service
        </button>
      </div>
      <div className='table-container'>
        <table id="roomServiceTable">
          <thead>
            <tr>
              <th>Extra Services</th>
            </tr>
          </thead>
          <tbody>
            {extraServices.map((extraService) => (
              <tr key={extraService.extraServiceID} onClick={() => handleEdit(extraService.serviceName)} data-item-name={extraService.serviceName}>
                <td className="menu-item">
                  <div className="name-desc">
                    <div className="item-name">{extraService.serviceName}</div>
                    <div className="description">({extraService.description})</div>
                  </div>
                  <span className="dots"></span>
                  <span className="price">{extraService.price.toFixed(2)}$</span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>    
    </div>
  );
}
