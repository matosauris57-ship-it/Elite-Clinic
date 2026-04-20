namespace Clinic_System.Application.Features.Doctors.Queries.Handlers
{
    public class DoctorListPagingQueryHandler : ResponseHandler , IRequestHandler<GetDoctorListPagingQuery, Response<PagedResult<GetDoctorListDTO>>>
    {
        private readonly IDoctorService doctorService;
        private readonly IMapper mapper;
        private readonly ICacheService cacheService;
        private readonly ILogger<DoctorListPagingQueryHandler> logger;

        public DoctorListPagingQueryHandler(
            IDoctorService doctorService,
            IMapper mapper,
            ILogger<DoctorListPagingQueryHandler> logger,
            ICacheService cacheService)
        {
            this.doctorService = doctorService;
            this.mapper = mapper;
            this.logger = logger;
            this.cacheService = cacheService;
        }

        public async Task<Response<PagedResult<GetDoctorListDTO>>> Handle(GetDoctorListPagingQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling GetDoctorListPagingQuery: PageNumber={PageNumber}, PageSize={PageSize}", request.PageNumber, request.PageSize);

            // أ. بناء مفتاح مميز للصفحة دي تحديداً
            string cacheKey = $"DoctorsList_Page_{request.PageNumber}_Size_{request.PageSize}";

            // ب. نسأل الـ Redis: "هل عندك الداتا دي؟"
            var cachedDoctors = await cacheService.GetDataAsync<PagedResult<GetDoctorListDTO>>(cacheKey);


            var pagedResult = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    // 👈 البلوك ده مش هيتنفذ غير لو الكاش فاضي تماماً، أو لو ده الريكويست "البطل" اللي بيجدد الكاش
                    logger.LogInformation("Fetching doctors from DATABASE for {CacheKey}", cacheKey);

                    var doctors = await doctorService.GetDoctorsListPagingAsync(request.PageNumber, request.PageSize, cancellationToken);

                    if (doctors?.Items.Any() != true)
                    {
                        return null; // لو مفيش داتا، هنرجع null عشان الكاش ميسجلش حاجة فاضية
                    }

                    var doctorsMapper = mapper.Map<List<GetDoctorListDTO>>(doctors.Items);
                    return new PagedResult<GetDoctorListDTO>(doctorsMapper, doctors.TotalCount, doctors.CurrentPage, doctors.PageSize);
                },
                TimeSpan.FromMinutes(30) // ده العمر المنطقي (Logical Expiry) اللي اتفقنا عليه
            );

            // 3. بنشيك على النتيجة النهائية
            if (pagedResult == null)
            {
                logger.LogWarning("No doctors found for PageNumber={PageNumber}, PageSize={PageSize}", request.PageNumber, request.PageSize);
                return NotFound<PagedResult<GetDoctorListDTO>>();
            }

            return Success(pagedResult);
        }
    }
}
