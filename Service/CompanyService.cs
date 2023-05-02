using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class CompanyService : ICompanyService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public CompanyService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repositoryManager = repository;
            _loggerManager = logger;
            _mapper = mapper;
        }


        public IEnumerable<CompanyDto> GetAllCompanies(bool trackChanges)
        {
            var companies =
                    _repositoryManager.Company.GetAllCompanies(trackChanges);

            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return companiesDto;
        }

        public CompanyDto GetCompany(Guid id, bool trackChanges) {
            var company = _repositoryManager.Company.GetCompany(id, trackChanges);

            if(company == null)
            {
                throw new CompanyNotFoundException(id);
            }

            var companyDto = _mapper.Map<CompanyDto>(company);
            return companyDto;
        }

        public CompanyDto CreateCompany(CompanyForCreationDto company)
        {
            var companyEntity = _mapper.Map<Company>(company);
            
            _repositoryManager.Company.CreateCompany(companyEntity);
            _repositoryManager.Save();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return companyToReturn;
        }

        public IEnumerable<CompanyDto> GetByIds(IEnumerable<Guid> ids, bool trackChanges)
        {
            if (ids == null)
                throw new IdParametersBadRequestException();

            var companyEntities = _repositoryManager.Company.GetByIds(ids, trackChanges);
            if (ids.Count() != companyEntities.Count())
                throw new CollectionByIdsBadRequestException();

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            return companiesToReturn;
        }

        (IEnumerable<CompanyDto> companies, string ids) CreateCompanyCollection
            (IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection == null)
                throw new CompanyCollectionBadRequest();

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var companyEntity in companyEntities)
            {
                _repositoryManager.Company.CreateCompany(companyEntity);
            }

            _repositoryManager.Save();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return (companies : companyCollectionToReturn, ids: ids);
        }
    }
}
